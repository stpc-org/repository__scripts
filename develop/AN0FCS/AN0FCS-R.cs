/***************************************************************************************************
* 
* ### AN0FCS's Not Only Fire Control System ###
* ### AN0FCS | "匿名者" 火控系统脚本 ------ ###
* ### Version 0.0.0 | by SiriusZ-BOTTLE --- ###
* ### STPC旗下SCPT工作室开发, 欢迎加入STPC  ###
* ### STPC主群群号:320461590 我们欢迎新朋友 ###
* 
* 
***************************************************************************************************/


//#define DEVELOP

#if DEVELOP
//用于IDE开发的 using 声明
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace AN0_RADAR_DEV
{
	class Program : MyGridProgram
	{
#endif

		#region 脚本字段

		//字符串 脚本版本号
		readonly string str__script_version = "AN0-RADAR V0.0.0-BETA ";

		//字符串 内部共享广播信道 (同步不同脚本实例的对象, 共享)
		readonly string string__inner_cast_channel = "AN0-R-SCRIPT-COMMUNICATION-CHANNEL-#0";
		//字符串 指示器信道通信后缀
		readonly string suffix_string__scanning_coordinate = "-SCANNING-COORDINATE";

		//数组 运行时字符显示
		readonly string[] array__runtime_chars = new string[]
		{
			"",
			"",
			"R",
			"RU",
			"RUN",
			"RUNN",
			"RUNNI",
			"RUNNIN",
			"RUNNING",
			"RUNNING",
		};

		//名称 脚本核心编组
		string name__script_core_group = "AN0-RADAR";

		//标记 是否 锁定自动炮塔的目标
		bool flag__enable_auto_turret_target_locking = true;
		//标记 是否 当离线 (无用户操控) 时广播目标
		bool flag__enable_cast_targets_off_line = true;

		// 标记 是否 开启局部锁定 (打击) (锁定时仍然使用目标中心位置锁定, 广播时追踪扫描时的位置)
		bool flag__enable_local_locking = false;
		//标记 是否 启用无限投射
		bool flag__enable_unlimited_raycast = true;
		// 标记 是否 接收扫描坐标
		bool flag__receiving_scanning_coordinate = true;

		// 全局目标过滤器

		//标记 是否 忽略玩家
		bool flag__ignore_players = false;
		//标记 是否 忽略火箭弹
		bool flag__ignore_rockets = false;
		//标记 是否 忽略流星
		bool flag__ignore_meteors = false;
		//标记 是否 忽略小型网格
		bool flag__ignore_small_grids = false;
		//标记 是否 忽略大型网格
		bool flag__ignore_large_grids = false;
		//标记 是否 忽略友方
		bool flag__ignore_the_friendly = false;
		//标记 是否 忽略中立方
		bool flag__ignore_the_neutral = false;
		//标记 是否 忽略敌对方
		bool flag__ignore_the_enemy = false;

		//周期 自动检查
		int period__auto_check = 60;
		//周期 更新输出
		int period__update_output = 30;
		//周期 更新目标序列
		int period__update_targets_sequence = 10;
		//周期 更新自动炮塔对象
		int period__update_targets_of_auto_turrets = 1;

		//距离 最小距离 (低于此距离的目标不会被摄像头锁定)
		double distance_min = 10;
		//距离 扫描距离
		double distance_scan = 2000;
		//度数 最大射击误差角 (大于这个值会停止射击)
		double degree__max_error_on_fire = 3;


		// 以上是脚本配置字段


		//列表 显示单元
		List<DisplayUnit> list__display_units = new List<DisplayUnit>();

		bool flag__custom_tgt_locked;//标记 用户自定义目标被锁定

		//计数 脚本运行命令次数
		long count__run_cmd = 0;
		//计数 更新
		long count_update = 0;
		//索引 当前字符图案
		long index__crt_char_pattern = 0;
		//次数 距离下一次 字符图案更新 (times before next)
		int time__before_next_char_pattern_update = 0;
		//次数 距离下一次 更新输出
		int time__before_next_output_update = 0;
		//次数 距离下一次 扫描
		int time__before_next_scan = 0;
		//次数 距离下一次 序列更新
		int time__before_next_seq_u = 0;

		//时间戳(脚本全局, ms)
		long timestamp = 0;
		//时间段
		long time_span;

		//增量 每一帧的增量
		double increment__per_frame = 0;
		//射程变化量(持续取平均)
		double variation_range;
		//摄像头射程总量(每一帧更新)
		double sum_range;

		//当前位置, 该值被视作脚本所在的实体的核心位置
		Vector3D position;
		//向量3D 脚本(扫描)目标位置
		Vector3D vector__target_pisition = new Vector3D();

		//目标 用户目标 (摄像头锁定
		Target target = new Target();
		//列表 自动炮台目标
		Dictionary<long, Target> dict__targets_of_auto_turrets;
		//列表 按速度排序(ASC)
		List<Target> list__tgts_order_by_speed = new List<Target>();
		//列表 按距离排序(ASC)
		List<Target> list__tgts_order_by_distance = new List<Target>();

		//字符串构建器 标题信息
		StringBuilder string_builder__title = new StringBuilder();
		//字符串构建器 脚本信息
		StringBuilder string_builder__script_info = new StringBuilder();

		//字符串构建器 目标信息
		StringBuilder string_builder__targets = new StringBuilder();
		//字符串构建器 测试信息
		StringBuilder string_builder__test_info = new StringBuilder();

		//对象管理器
		ObjectsManager manager_objects;

		IMyBroadcastListener listener;

		//脚本配置
		DataConfig config_script;
		//脚本配置集合
		DataConfigSet config_set__script;


		#endregion

		#region 脚本入口

		/**************************************************************************
		* 构造函数 Program()
		**************************************************************************/
		public Program()
		{
			Echo("<execute> init");

			//初始化脚本
			init_script();

			Echo("<done> init");
		}

		/**************************************************************************
		* 入口函数 Main()
		**************************************************************************/
		void Main(string str_arg, UpdateType type_update)
		{
			switch (type_update)
			{
				case UpdateType.Terminal:
				case UpdateType.Trigger:
				case UpdateType.Script:
					{
						run_command(str_arg);
					}
					break;
				case UpdateType.Update1:
					{
						update_script();
					}
					break;
			}
		}

		#endregion

		#region 成员函数

		// 脚本更新
		void update_script()
		{
			// 更新摄像头数据
			update_camera_data();

			// 更新时间戳
			update_timestamp();

			//当前位置设为编程块所在位置
			position = Me.GetPosition();

			// 获取当前帧自动炮塔的目标
			foreach (var turret in manager_objects.list__auto_turrets)
			{
				if (turret.HasTarget)//存在目标
				{
					// 获取目标实体信息
					var entity = turret.GetTargetedEntity();
					if (dict__targets_of_auto_turrets.ContainsKey(entity.EntityId))
						// 存在目标则更新
						dict__targets_of_auto_turrets[entity.EntityId].set(timestamp, entity, position);
					else
						// 不存在则新建
						dict__targets_of_auto_turrets[entity.EntityId] = new Target(timestamp, entity, position);
				}
			}

			try
			{
				if (flag__custom_tgt_locked)
					flag__custom_tgt_locked = lock_target();//锁定目标
			}
			catch (Exception e)
			{
				Me.CustomData = e.ToString();
			}

			display_info();//信息显示
			++count_update;//更新次数+1
		}

		// 更新摄像头数据
		void update_camera_data()
		{
			var tmp = sum_range;
			sum_range = 0;
			// 更新当前帧摄像头总射程
			foreach (var i in manager_objects.list_cameras)
				sum_range += i.AvailableScanRange;
			// 更新射程变化量 (平滑)
			variation_range = TK.cal_smooth_avg(variation_range, sum_range - tmp);
		}

		// 更新时间戳
		void update_timestamp()
			=> timestamp += time_span = Runtime.TimeSinceLastRun.Milliseconds;

		//执行指令
		void run_command(string str_arg)
		{

		}


		// 扫描 (朝指定坐标投射)
		bool scan(Vector3D vector_coordinate)
		{
			// 检查摄像头数量, 没有摄像头直接退出
			if (manager_objects.list_cameras.Count == 0)
				return false;

			// 目标实体信息
			var info_entity = new MyDetectedEntityInfo();
			// 偏航, 俯仰
			float yaw, pitch;
			// 目标距离
			double distance = 0;

			if (time__before_next_scan <= 0)
			{
				foreach (var camera in manager_objects.list_cameras)
				{
					// 投射朝向
					var orientation = vector_coordinate - camera.WorldMatrix.Translation;

					// 全局朝向转 yaw+pitch
					TK.global_to_yaw_pitch(out yaw, out pitch, camera.WorldMatrix, orientation);

					// 检查是否超过了当前摄像头的射界
					if (Math.Abs(yaw) > camera.RaycastConeLimit || Math.Abs(pitch) > camera.RaycastConeLimit)
						continue;

					// 进行投射
					info_entity = camera.Raycast(1.05 * orientation.Length(), pitch, yaw);
					if (manager_objects.set__self_ids.Contains(info_entity.EntityId))
						// 扫描到自身网格, 跳过
						continue;
					// 计算目标距离摄像头的距离
					distance = (info_entity.Position - camera.WorldMatrix.Translation).Length();
					break;
				}
				// 设置下一次扫描的冷却时间
				if (flag__enable_unlimited_raycast)
					time__before_next_scan = 1;
				else if (info_entity.IsEmpty())
					time__before_next_scan = variation_range > 0 ? (int)(distance_scan / variation_range) : int.MaxValue;
				else
					// 当前消耗值 / 摄像头能量增量
					time__before_next_scan = (int)(distance / variation_range);
				// 注:
				// 除法计算后转为整型可能导致值为0的情况, 之后再次--导致值为-1, 因此入口条件设为<=0
			}
			--time__before_next_scan;

			// 扫描到对象且距离超过阈值
			if (!info_entity.IsEmpty() && distance > distance_min)
			{
				switch (info_entity.Type)//类型过滤器
				{
					case MyDetectedEntityType.CharacterHuman://人类角色
					case MyDetectedEntityType.CharacterOther://非人角色
						if (flag__ignore_players) return false; break;
					case MyDetectedEntityType.Missile://小型网格
						if (flag__ignore_rockets) return false; break;
					case MyDetectedEntityType.Meteor://小型网格
						if (flag__ignore_meteors) return false; break;
					case MyDetectedEntityType.SmallGrid://小型网格
						if (flag__ignore_small_grids) return false; break;
					case MyDetectedEntityType.LargeGrid://大型网格
						if (flag__ignore_large_grids) return false; break;
					case MyDetectedEntityType.FloatingObject://漂浮物
					case MyDetectedEntityType.Asteroid://小行星
					case MyDetectedEntityType.Planet://行星
					case MyDetectedEntityType.Unknown://未知
					case MyDetectedEntityType.None://空
						return false;//被忽略的类型
				}
				switch (info_entity.Relationship)//关系过滤器
				{
					case MyRelationsBetweenPlayerAndBlock.Friends://友方
					case MyRelationsBetweenPlayerAndBlock.FactionShare://阵营共享
						if (flag__ignore_the_friendly) return false; break;
					case MyRelationsBetweenPlayerAndBlock.Neutral://中立方
					case MyRelationsBetweenPlayerAndBlock.NoOwnership://无归属
						if (flag__ignore_the_neutral) return false; break;
					case MyRelationsBetweenPlayerAndBlock.Enemies://敌对方
						if (flag__ignore_the_enemy) return false; break;
				}
				target.set(timestamp, info_entity, position); return true;
			}
			return false;
		}

		//尝试锁定目标 (返回是否成功)
		bool lock_target()
		{
			Vector3D pv, pa, pa_avl, pm, pm2, position = target.position;
			//时间
			var time = (timestamp - target.timestamp) / 1000f;
			pa_avl = pa = pv = position + target.v * time;
			if (!target.acc_average.IsZero()) { pa += 0.5 * target.acc * time * time; pa_avl += 0.5 * target.acc_average * time * time; }
			// 中点
			pm = (pa + pv) / 2; pm2 = (position + pm) / 2;
			// 五点探测锁定
			if (scan(pa) || scan(pa_avl) || scan(pv) || scan(pm) || scan(pm2))
			{
				if (!flag__custom_tgt_locked)
					variation_range = increment__per_frame;
				return true;
			}
			else
				return false;//丢失目标
		}

		//输出信息 输出信息到编程块终端和LCD
		void display_info()
		{
			if (!TK.check_time(ref time__before_next_output_update, period__update_output))
				return;
			//清空
			string_builder__title.Clear(); string_builder__script_info.Clear();
			string_builder__title.Append("<script> ANO-RADAR V0.0.0 " + array__runtime_chars[index__crt_char_pattern]);
			string_builder__script_info.Append
			(
				$"\n<count_update | TS | TSP> {count_update} {timestamp} {time_span}"
				+ $"\n<count_tgt> {dict__targets_of_auto_turrets.Keys.Count + (flag__custom_tgt_locked ? " LOCKED" : " NO TGT")}"
				+ $"\n<orientation_global> {vector__target_pisition.ToString("f2")}"
				+ $"\n<count_all_blocks> {manager_objects.list_blocks.Count}"
				+ $"\n<count_controllers> {manager_objects.list_controllers.Count}"
				+ $"\n<count_cameras> {manager_objects.list_cameras.Count}"
				+ $"\n<count_auto_turrets> {manager_objects.list__auto_turrets.Count}\n"
			);

			string_builder__targets.Append($"<camera_target>\n{target}");
			foreach (var target in dict__targets_of_auto_turrets.Values)
				string_builder__targets.Append(target);
			// 显示内容
			Echo(string_builder__title.ToString());
			Echo(string_builder__script_info.ToString());
			Echo(string_builder__targets.ToString());

			//遍历显示单元
			foreach (var item in list__display_units)
			{
				if (item.flag_graphic)
				{
					//图形化显示
					if (item.lcd.ContentType != ContentType.SCRIPT)
						continue;
				}
				else
				{
					//非图形化显示
					if (item.lcd.ContentType != ContentType.TEXT_AND_IMAGE)
						continue;
					switch (item.mode_display)
					{
						case DisplayUnit.DisplayMode.Script:
							item.lcd.WriteText(string_builder__title);
							item.lcd.WriteText(string_builder__script_info, true);
							break;
						case DisplayUnit.DisplayMode.Targets:
							{

							}
							break;
						case DisplayUnit.DisplayMode.None:
							item.lcd.WriteText("<warning> illegal custom data in this LCD\n<by> script ANO-R");
							break;
					}
				}
			}

			//显示测试信息
			Echo("\n<debug>\n" + string_builder__test_info.ToString());

			if (TK.check_time(ref time__before_next_char_pattern_update, 1))
				if ((++index__crt_char_pattern) >= array__runtime_chars.Length)
					index__crt_char_pattern = 0;
		}

		//初始化脚本
		void init_script()
		{
			// 初始化脚本配置
			init_script_config();
			// 检查配置合法性
			string str_error = check_config();
			// 非法配置输出信息
			if (str_error != null)
				Echo(str_error);

			// 构建对象管理器
			manager_objects = new ObjectsManager(this);
			// 输出初始化过程中的信息
			Echo(manager_objects.string_builder__init_info.ToString());

			// 初始化配置
			config_script.init_config();
			// 输出配置初始化过程中的信息
			Echo(config_script.string_builder__error_info.ToString());

			// 注册广播监听器
			listener = IGC.RegisterBroadcastListener(string__inner_cast_channel + suffix_string__scanning_coordinate);

			// 初始化脚本显示单元
			init_script_display_units();

			if (manager_objects.list_cameras.Count > 0)//计算最速扫描周期
				increment__per_frame = manager_objects.list_cameras.Count * 2000.0 / 60 / 4;

			if (str_error == null && !config_script.flag__config_error)//检查是否出现错误
				Runtime.UpdateFrequency = UpdateFrequency.Update1;//设置执行频率 每1帧一次
		}


		//拆分字符串
		string[] split_string(string str)
		{
			string[] list_str = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			return list_str;
		}

		//检查数值
		bool check_value(int value) => value > 0 && value < 1001;

		//初始化脚本显示单元
		void init_script_display_units()
		{
			foreach (var item in manager_objects.list_displayers)//遍历显示器
			{
				//拆分显示器的用户数据
				string[] array_str = split_string(item.CustomData);
				bool flag_illegal = false;
				int offset = 0;

				DisplayUnit unit = new DisplayUnit(item);

				if (array_str.Length == 0)
					//用户数据为空
					unit.mode_display = DisplayUnit.DisplayMode.Script;
				else
				{
					if (array_str[array_str.Length - 1].Equals("graphic"))
					{
						offset = 1;
						unit.flag_graphic = true;
					}
					//用户数据不为空
					switch (array_str[0])
					{
						case "graphic":
						case "page0":
							unit.mode_display = DisplayUnit.DisplayMode.Script;
							break;
						case "targets":
							unit.mode_display = DisplayUnit.DisplayMode.Targets;
							break;
						default:
							unit.mode_display = DisplayUnit.DisplayMode.None;
							break;
					}
				}

				if (flag_illegal)
					unit.mode_display = DisplayUnit.DisplayMode.None;

				//自动设置LCD的显示模式
				if (unit.flag_graphic)
				{
					item.ContentType = ContentType.SCRIPT;//设置为 脚本模式
					item.Script = "";//脚本设为 None
					item.ScriptBackgroundColor = Color.Black;//黑色背景色
				}
				else
					item.ContentType = ContentType.TEXT_AND_IMAGE;//设置为 文字与图片模式

				//添加到列表
				list__display_units.Add(unit);
			}
		}

		//初始化脚本配置
		void init_script_config()
		{
			//脚本配置实例
			config_script = new DataConfig("ANO-L");
			config_script.set_string(() => Me.CustomData, (x) => Me.CustomData = x);

			config_set__script = new DataConfigSet("AN0-R CONFIGURATION");

			//添加配置项
			config_set__script.add_item((new Variant(nameof(name__script_core_group), Variant.VType.String, () => name__script_core_group, x => { name__script_core_group = (string)x; })));

			config_set__script.add_item((new Variant(nameof(flag__enable_auto_turret_target_locking), Variant.VType.Bool, () => flag__enable_auto_turret_target_locking, x => { flag__enable_auto_turret_target_locking = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__enable_cast_targets_off_line), Variant.VType.Bool, () => flag__enable_cast_targets_off_line, x => { flag__enable_cast_targets_off_line = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__enable_unlimited_raycast), Variant.VType.Bool, () => flag__enable_unlimited_raycast, x => { flag__enable_unlimited_raycast = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_players), Variant.VType.Bool, () => flag__ignore_players, x => { flag__ignore_players = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_rockets), Variant.VType.Bool, () => flag__ignore_rockets, x => { flag__ignore_rockets = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_meteors), Variant.VType.Bool, () => flag__ignore_meteors, x => { flag__ignore_meteors = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_small_grids), Variant.VType.Bool, () => flag__ignore_small_grids, x => { flag__ignore_small_grids = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_large_grids), Variant.VType.Bool, () => flag__ignore_large_grids, x => { flag__ignore_large_grids = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_the_friendly), Variant.VType.Bool, () => flag__ignore_the_friendly, x => { flag__ignore_the_friendly = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_the_neutral), Variant.VType.Bool, () => flag__ignore_the_neutral, x => { flag__ignore_the_neutral = (bool)x; })));
			config_set__script.add_item((new Variant(nameof(flag__ignore_the_enemy), Variant.VType.Bool, () => flag__ignore_the_enemy, x => { flag__ignore_the_enemy = (bool)x; })));

			//config_set__script.add_item(new Variant(nameof(period__auto_check), Variant.VType.Int, () => period__auto_check, x => { period__auto_check = (int)x; }));
			//config_set__script.add_item(new Variant(nameof(period__update_output), Variant.VType.Int, () => period__update_output, x => { period__update_output = (int)x; }));

			config_set__script.add_item(new Variant(nameof(distance_min), Variant.VType.Float, () => distance_min, x => { distance_min = (double)x; }));
			config_set__script.add_item(new Variant(nameof(distance_scan), Variant.VType.Float, () => distance_scan, x => { distance_scan = (double)x; }));
			config_set__script.add_item(new Variant(nameof(degree__max_error_on_fire), Variant.VType.Float, () => degree__max_error_on_fire, x => { degree__max_error_on_fire = (double)x; }));
			//config_set__script.add_item(new Variant(nameof(vector_coeff), Variant.VType.V3D, () => vector_coeff, x => { vector_coeff = (Vector3D)x; }));

			config_script.add_config_set(config_set__script);

			//初始化配置
			//config_script.parse_custom_data();
			config_script.init_config();

		}

		//检查脚本配置(配置合法返回null, 否则返回错误消息)
		string check_config()
		{
			if (name__script_core_group.Length == 0)
				return "<error_config> key tag of group name is empty";
			if (!check_value(period__auto_check)
				|| !check_value(period__update_output))
				return "<error_config> period cannot be less than 1 or more than 1000";

			return null;
		}

		#endregion

		#region 类型定义

		//类 显示单元
		class DisplayUnit
		{
			//枚举 显示模式
			public enum DisplayMode
			{
				Script,//脚本主要信息
				Targets,//脚本的全部目标信息
				None,//不显示内容
			}

			//LCD显示器
			public IMyTextPanel lcd;

			public DisplayMode mode_display = DisplayMode.Script;

			//索引 开始 (列表容器中的索引)
			public int index_begin = -1;
			//索引 末尾 (列表容器中的索引)
			public int index_end = -1;
			//标记 是否图形化显示
			public bool flag_graphic = false;

			//构造函数
			public DisplayUnit(IMyTextPanel _lcd, int _index_begin = -1, int _index_end = -1, bool _flag_graphic = false)
			{
				lcd = _lcd;
				flag_graphic = _flag_graphic;
				index_begin = _index_begin;
				index_end = _index_end;
			}
		}

		//类 目标
		class Target
		{
			public long timestamp { get; private set; }// 时间戳 (目标被发现时的时间戳)
			public MyDetectedEntityInfo info_entity;// 实体信息
			public Vector3D acc { get; private set; }
			public Vector3D acc_average { get; private set; }// 瞬时加速度和平均加速度
			public Vector3D position => info_entity.Position;// 位置
			public double distance { get; private set; }// 距离
			public double speed { get; private set; }// 速率
			public Target(long _ts, MyDetectedEntityInfo _i, Vector3D _p) { set(_ts, _i, _p); }//构造函数
			public Target() { timestamp = -1; }
			public long id => info_entity.EntityId; public Vector3D v => info_entity.Velocity;
			public string name => info_entity.Name;// 名称
			public Vector3D next_tick(long ts) => position + info_entity.Velocity * (ts / 1000f);//更新位置
			public void set(long _ts, MyDetectedEntityInfo _i, Vector3D _p)//设置对象
			{
				if (info_entity.EntityId == _i.EntityId)
				{
					acc = (_i.Velocity - info_entity.Velocity) * 1000f / (_ts - timestamp);
					var t = TK.cal_smooth_avg(acc_average.Length(), acc.Length());
					acc_average = acc_average + t * (acc - acc_average);
				}
				else acc = acc_average = Vector3D.Zero;
				timestamp = _ts;
				info_entity = _i;
				distance = (_p - position).Length();
				speed = info_entity.Velocity.Length();
			}
			public string ToString() => timestamp < 0 ? "<target> invalid" :
				$"<target> --------------------"
				+ $"\n<name | type> {name} {info_entity.Type} "
				+ $"\n<id | timestamp> {id} {timestamp}"
				+ $"\n<distance | position> {TK.nf(distance)} {position.ToString("f2")}"
				+ $"\n<speed | velocity> {TK.nf(speed)} {info_entity.Velocity.ToString("f2")}"
				+ $"\n<acc | acc_avg> {TK.nf(acc.Length())} {TK.nf(acc_average.Length())}\n";
			public static string get_title() => "name id type distance speed acc acc_avg";
		}


		/**************************************************************************
		* 类 ObjectsManager
		* 对象管理器
		* 实例中包含本脚本所需的全部对象列表
		**************************************************************************/
		class ObjectsManager
		{

			IMyBlockGroup group__script_core = null;//编组 脚本核心方块

			//列表 所有方块
			public List<IMyTerminalBlock> list_blocks { get; private set; } = new List<IMyTerminalBlock>();
			//列表 全部机械连接方块(排除悬架)
			public List<IMyMechanicalConnectionBlock> list_mcbs { get; private set; } = new List<IMyMechanicalConnectionBlock>();
			//列表 全部控制器
			public List<IMyShipController> list_controllers { get; private set; } = new List<IMyShipController>();
			//列表 全部摄像头
			public List<IMyCameraBlock> list_cameras { get; private set; } = new List<IMyCameraBlock>();
			//列表 全部自动炮塔
			public List<IMyLargeTurretBase> list__auto_turrets { get; private set; } = new List<IMyLargeTurretBase>();
			//列表 显示器
			public List<IMyTextPanel> list_displayers { get; private set; } = new List<IMyTextPanel>();
			//哈希表 自身网格的ID
			public HashSet<long> set__self_ids { get; private set; } = new HashSet<long>();
			//字典 根据网格快速检索节点索引
			Dictionary<IMyCubeGrid, int> dict__grids_index = new Dictionary<IMyCubeGrid, int>();

			Program p = null;
			//字符串构建器 初始化信息
			public StringBuilder string_builder__init_info { get; private set; } = new StringBuilder();

			//构造函数
			public ObjectsManager(Program _program)
			{
				//成员赋值
				p = _program;
				//获取脚本核心编组
				group__script_core = p.GridTerminalSystem.GetBlockGroupWithName(p.name__script_core_group);
				if (group__script_core == null) return;
				group__script_core.GetBlocks(list_blocks);
				group__script_core.GetBlocksOfType(list_controllers);
				group__script_core.GetBlocksOfType(list_cameras);
				group__script_core.GetBlocksOfType(list__auto_turrets);
				group__script_core.GetBlocksOfType(list_displayers);
				if (p.flag__enable_unlimited_raycast)
					foreach (var i in list_cameras)
					{
						i.EnableRaycast = true;//启用投射
						i.Raycast(1d / 0d);//进行一次测试性投射
					}

				//获取全部机械连接方块(电机, 铰链, 活塞, 悬架)
				p.GridTerminalSystem.GetBlocksOfType(list_mcbs);

				int index = 0;
				//获取全部网格 构建节点列表和节点索引
				foreach (var item in list_mcbs)
				{
					if (!dict__grids_index.ContainsKey(item.CubeGrid))
					{
						dict__grids_index[item.CubeGrid] = index++;
						set__self_ids.Add(item.CubeGrid.EntityId);
						string_builder__init_info.Append($"<info> node {index - 1} {item.CubeGrid.CustomName}\n");
					}
					if (item.TopGrid != null && !dict__grids_index.ContainsKey(item.TopGrid))
					{
						dict__grids_index[item.TopGrid] = index++; set__self_ids.Add(item.TopGrid.EntityId);
						string_builder__init_info.Append($"<info> node {index - 1} {item.TopGrid.CustomName}\n");
					}
				}

				generate_init_info();
			}

			//生成初始化信息
			private void generate_init_info()
			{
				string_builder__init_info.Append($"<info> list_blocks.Count = {list_blocks.Count}\n");
				string_builder__init_info.Append($"<info> list_cameras.Count = {list_cameras.Count}\n");
				string_builder__init_info.Append($"<info> list_controllers.Count = {list_controllers.Count}\n");
				string_builder__init_info.Append($"<info> list__auto_turrets.Count = {list__auto_turrets.Count}\n");
				string_builder__init_info.Append($"<info> list_displayers.Count = {list_displayers.Count}\n");

				//buinder_str__init_info.Append($"<info> list__other_blocks.Count = {list__other_blocks.Count}\n");
			}
		}

		static class TK
		{
			//计算平滑均值 (上个平滑值, 当前值, 增量(增量为null使用默认的减法计算))
			//逼近新的值, 变化越大惩罚越大, inc为倍数增量的比率0.5*[0,1]
			public static double cal_smooth_avg(double l, double c, double? i = null)
			{
				double inc = 0.5 * ((inc = Math.Abs((i == null ? c - l : i.Value) / l)) > 1 ? 1 : inc); return inc * l + (1 - inc) * c;
			}

			//全局(朝向)转 yaw pitch
			public static void global_to_yaw_pitch(out float yaw, out float pitch, MatrixD matrix, Vector3D tgt)
			{
				Vector3D l = matrix.Left, u = matrix.Up, f = matrix.Forward;
				var p2u = Vector3D.ProjectOnPlane(ref tgt, ref u);//计算投影
				double num = Vector3D.Dot(p2u, f) / (p2u.Length() * f.Length());
				if (num > 1.0) num = 1.0; if (num < -1.0) num = -1.0;
				yaw = (float)(Math.Acos(num) * 180 / Math.PI); if (Vector3D.Dot(l, p2u) > 0) yaw = -yaw;
				num = Vector3D.Dot(p2u, tgt) / (p2u.Length() * tgt.Length());
				if (num > 1.0) num = 1.0; if (num < -1.0) num = -1.0;
				pitch = (float)(Math.Acos(num) * 180 / Math.PI); if (Vector3D.Dot(u, tgt) < 0) pitch = -pitch;
			}

			//检查是否到时间执行某步骤
			public static bool check_time(ref int time, int period)
			{
				bool res = false;
				if (time == 0)
				{
					time = period;
					res = true;
				}
				--time;
				return res;
			}

			//转字符串(保留2位小数)
			public static string nf(double d) => d.ToString("f2");
		}

		#endregion

		#region 配置通用

		/**************************************************************************
		* 类 自定义数据配置
		* 自定义数据配置(下简称CD配置)使用目标方块的自定义数据来进行脚本配置
		* 支持动态配置, 注释等, 功能强大
		**************************************************************************/

		//类 变量
		public class Variant
		{
			//枚举 变量类型
			public enum VType { None, Unknown, Bool, Float, Int, String, V3D, }//空, 未知, 布尔, 浮点, 整数, 字符串, V3D
			public readonly string name = "undefined";//名称
			public VType type_v = VType.None;
			public object value;
			public Func<object> get { get; private set; } = null;//读委托
			public Action<object> set { get; private set; } = null;//写委托

			//构造函数 传递名称和类型
			public Variant(string _name, VType _type = VType.String, Func<object> _getter = null, Action<object> _setter = null)
			{
				name = _name; type_v = _type; get = _getter; set = _setter;
				if (get == null && set == null)
				{
					get = () => value;
					switch (type_v)
					{
						case VType.Bool: value = new bool(); set = x => { value = (bool)x; }; break;
						case VType.Int: value = new long(); set = x => { value = (long)x; }; break;
						case VType.Float: value = new double(); set = x => { value = (double)x; }; break;
						case VType.String: value = ""; set = x => { value = (string)x; }; break;
						case VType.V3D: value = new Vector3D(); set = x => { value = (Vector3D)x; }; break;
					}
				}
			}

			public Variant set_value(Object value_new) { set(value_new); return this; }
			public Object get_value() => get();

			//解析字符串
			public bool parse_string(string value_str)
			{
				switch (type_v)
				{
					case VType.Bool: bool b; if (bool.TryParse(value_str, out b)) set(b); else return false; break;
					case VType.Int: long l; if (long.TryParse(value_str, out l)) set(l); else return false; break;
					case VType.Float: double d; if (double.TryParse(value_str, out d)) set(d); else return false; break;
					case VType.String: set(value_str); break;
					case VType.V3D: Vector3D v3d; if (Vector3D.TryParse(value_str, out v3d)) set(v3d); else return false; break;
				}
				return true;
			}

			override public string ToString() => $"{name} = {(get == null ? value.ToString() : get().ToString())}\n";
		}

		//管理对象
		class DataConfig
		{
			//函数指针 读
			public Func<string> get { get; private set; }
			//函数指针 写
			public Action<string> set { get; private set; }

			//标识符 域
			public static string identifier_scope_0 = "##########";
			public static string identifier_scope_1 = "##########";
			//标识符 标题
			public static string identifier_set_0 = "[";
			public static string identifier_set_1 = "]";
			//标识符 注解
			public static string identifier_annotation_0 = "@";
			public static string identifier_annotation_1 = "";
			//标记 是否自动创建确缺失的变量
			public static bool flag__create_missing_variant = false;

			//配置标题
			public string title_scope { get; private set; }

			//索引 范围开始
			int index_begin = -1;
			//索引 范围结束
			int index_end = -1;

			//映射表 配置项集合
			Dictionary<string, DataConfigSet> dict__config_sets = new Dictionary<string, DataConfigSet>();
			//映射表 字符串内容
			Dictionary<string, List<string>> dict__str_contents = new Dictionary<string, List<string>>();
			//字符串构建器 数据
			public StringBuilder string_builder__data { get; private set; } = new StringBuilder();
			//字符串构建器 错误信息
			public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();
			//标记 从字符串内容中构建配置
			public bool flag__construct_from_str { get; private set; } = false;
			//标记 配置中发现错误(存在错误时不会覆盖写入)
			public bool flag__config_error { get; private set; } = false;

			public DataConfig(string _title_scope)
			{
				title_scope = _title_scope;
			}

			public void set_string(Func<string> getter, Action<string> setter)
			{
				get = getter; set = setter; string_builder__data.Clear();
			}

			//初始化配置
			public void init_config()
			{
				parse_str_data();//解析自定义数据
				if (!flag__config_error)//检查CD配置是否存在错误
					write_to_string();//写入自定义数据
			}

			//添加配置集
			public bool add_config_set(DataConfigSet set)
			{
				if (dict__config_sets.ContainsKey(set.title__config_set))
					return false;
				dict__config_sets.Add(set.title__config_set, set);
				return true;
			}

			//解析字符串数据(拆分)
			public void parse_str_data()
			{
				string[] array_lines = get().Split('\n');//以换行符拆分
				string pattern_set = $"\\[ (.+) \\]";//正则表达式
				string pattern_scope = $"{identifier_scope_0} {title_scope} {identifier_scope_1}";//正则表达式
				var regex_set = new System.Text.RegularExpressions.Regex(pattern_set);
				var regex_scope = new System.Text.RegularExpressions.Regex(pattern_scope);
				string title_crt = "";
				bool flag__config_start = false;
				int index = 0, index_line = 0, len_line;
				for (; index_line < array_lines.Length; ++index_line, index += len_line)
				{
					var line = array_lines[index_line]; len_line = line.Length + 1;
					if (line.Length == 0) continue;
					var match_scope = regex_scope.Match(line);//scope边界匹配
					var match_set = regex_set.Match(line);//配置集标题匹配
					if (flag__config_start)//已开始
					{
						if (match_scope.Success)
						{
							index_end = index + line.Length;
							break;
						}
					}
					else//未开始
					{
						if (match_scope.Success)//匹配成功
						{
							index_begin = index;
							flag__config_start = true;
						}
						continue;
					}
					if (match_set.Success)//匹配成功, 新的配置集
						dict__str_contents[title_crt = match_set.Groups[1].ToString()] = new List<string>();
					else if (dict__str_contents.ContainsKey(title_crt))
						dict__str_contents[title_crt].Add(line);//条目添加到容器中
					else
					{
						string_builder__error_info.Append($"<error> illegal CD config data at line #{index_line}: \n{line}\n");
						flag__config_error = true; break;
					}
				}
				foreach (var pair in dict__str_contents)
				{
					if (!dict__config_sets.ContainsKey(pair.Key))
					{
						if (flag__construct_from_str)
						{
							var set = new DataConfigSet();
							this.add_config_set(set);
						}
					}
					else if (!dict__config_sets[pair.Key].parse_str_data(pair.Value))
					{
						string_builder__error_info.Append($"<error> illegal config item in config set: [{pair.Key}]\n{dict__config_sets[pair.Key].string_builder__error_info}\n");
						flag__config_error = true; break;
					}

				}
			}

			//写入字符串
			public void write_to_string()
			{
				if (index_begin >= 0)
					string_builder__data.Append(get().Substring(0, index_begin));
				string_builder__data.Append(identifier_scope_0 + $" {title_scope} " + identifier_scope_1 + "\n");
				foreach (var item in dict__config_sets)
				{
					item.Value.generate_string_data();
					string_builder__data.Append(item.Value.string_builder__data);
				}
				string_builder__data.Append(identifier_scope_0 + $" {title_scope} " + identifier_scope_1);
				if (index_end >= 0 && index_end < get().Length)
					string_builder__data.Append(get().Substring(index_end));
				set(string_builder__data.ToString());
			}
		}
		//CD配置集合
		class DataConfigSet
		{
			//标题 配置集
			public string title__config_set { get; private set; }

			//字典 项字典
			Dictionary<string, Variant> dict__config_items = new Dictionary<string, Variant>();

			//字符串构建器 数据
			public StringBuilder string_builder__data { get; private set; } = new StringBuilder();
			//字符串构建器 错误信息
			public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();

			//标记 配置中发现错误(存在错误时不会覆盖写入)
			public bool flag__config_error { get; private set; } = false;

			//构造函数
			public DataConfigSet(string title = "SCRIPT MAIN CONFIGURATION")
			{
				title__config_set = title;
			}

			//添加变量
			public void add_item(Variant v) => dict__config_items[v.name] = v;

			//获取变量 不存在返回null
			public Variant get_item(string name_item) => dict__config_items.ContainsKey(name_item) ? dict__config_items[name_item] : null;

			public Variant this[string name] { get { return get_item(name); } }

			//添加注释
			public bool add_annotation(string str_title)
			{
				//检查是否已经包含此配置项目
				if (dict__config_items.ContainsKey(str_title))
					return false;
				//添加到字典
				dict__config_items.Add(str_title, null);
				return true;
			}

			//解析字符串
			public bool parse_str_data(List<string> content)
			{
				int count = 0;

				foreach (var line in content)
				{
					++count;
					if (line.Length == 0) continue;//跳过空行
					if (line.StartsWith(DataConfig.identifier_annotation_0)) continue;
					//以等号拆分
					string[] pair = line.Split('=');
					//检查
					if (pair.Length != 2)
					{
						string_builder__error_info.Append($"<error> \"{line}\"(at line {count}) is not a legal config item");
						flag__config_error = true; continue;//跳过配置错误的行
					}

					//去除多余空格
					string name__config_item = pair[0].Trim();
					string str_value__config_item = pair[1].Trim();

					Variant variant;
					//尝试获取
					if (!dict__config_items.TryGetValue(name__config_item, out variant))
						if (DataConfig.flag__create_missing_variant)
						{
							//自动创建变量
							variant = new Variant(name__config_item);
							dict__config_items[name__config_item] = variant;
						}
						else
							continue;//不包含值, 跳过

					//解析并更新
					if (!variant.parse_string(str_value__config_item))
					{
						//解析失败
						string_builder__error_info.Append($"<error> \"{str_value__config_item}\"(at line {count}) is not a legal config value");
						flag__config_error = true;
					}
				}
				return !flag__config_error;
			}

			//生成字符串数据
			public void generate_string_data()
			{
				int count = 0;

				string_builder__data.Clear();
				string_builder__data.Append($"\n{DataConfig.identifier_set_0} {title__config_set} {DataConfig.identifier_set_1}\n");
				foreach (var pair in dict__config_items)
				{
					if (pair.Value != null) string_builder__data.Append(pair.Value);
					else
					{
						if (count != 0) string_builder__data.Append("\n");
						string_builder__data.Append($"{DataConfig.identifier_annotation_0} {pair.Key} {DataConfig.identifier_annotation_1}\n");
					}
					++count;
				}
				string_builder__data.Append("\n");
			}

		}

		#endregion

#if DEVELOP
	}
}
#endif