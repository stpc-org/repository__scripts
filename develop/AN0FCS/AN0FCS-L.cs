/***************************************************************************************************
*
* ### AN0FCS's Not Only Fire Control System - launcher ###
* ### AN0FCS-launcher | "匿名者" 火控系统启动器脚本 -- ###
* ### Version 0.0.0 | by SiriusZ-BOTTLE -------------- ###
* ### STPC旗下SCPT工作室开发, 欢迎加入STPC ----------  ###
* ### STPC主群群号:320461590 我们欢迎新朋友 ---------- ###
* 
* 
***************************************************************************************************/

//#define DEVELOP

#if DEVELOP
//用于IDE开发的 using 声明
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.Game;
using VRageMath;

namespace AN0FCS_LAUNCHER_DEV
{
	class Program : MyGridProgram
	{
#endif

		#region 脚本字段

		//字符串 脚本版本号
		readonly string str__script_version = "AN0FCS-LAUNCHER V0.0.0 ";

		//标签 脚本核心编组
		string tag__script_core_group = "AN0-CORE";
		//标签 高低转子(俯仰)
		string tag__elevation_stators = "_V";
		//标签 方向转子(偏航)
		string tag__azimuth_stators = "_H";
		//标签 高低机活塞网格(功能不推荐使用)
		//string tag__ele_pistons_gird = "ElevationPistonGrid";
		//标签 指示器(优先考虑)
		//string tag_indicator = "indicator";
		//标签 阻塞连通性
		string tag__blocking_connectivity = "blocked";
		//标签 射击(主要用于区分定时器)
		//string tag_fire = "fire";
		//标签 停火
		//string tag_hold = "hold";

		//以上是脚本配置字段

		//网格图结构
		ObjectsManager manager_objects;
		//脚本配置
		DataConfig config_script;
		//脚本配置集合
		DataConfigSet config_set__script;


		#endregion

		#region 脚本入口

		/***************************************************************************************************
		* 构造函数 Program()
		***************************************************************************************************/
		public Program()
		{
			Echo("<execute> init");

			//初始化脚本
			init_script();

			Echo("<done> init");
		}

		/***************************************************************************************************
		* 入口函数 Main()
		***************************************************************************************************/
		void Main(string str_arg, UpdateType type_update)
		{
			switch (type_update)
			{
				case UpdateType.Terminal:
				case UpdateType.Trigger:
				case UpdateType.Script:
					{
						//run_command(str_arg);
					}
					break;
				case UpdateType.Update1:
				case UpdateType.Update10:
				case UpdateType.Update100:
					{
						//update_script();
					}
					break;
			}
		}

		#endregion

		#region 成员函数

		//初始化脚本
		void init_script()
		{
			init_script_config();//初始化脚本配置
			string str_error = check_config();
			//检查配置合法性
			if (str_error != null) Echo(str_error);

			config_script.init_config();//初始化配置
			Echo(config_script.string_builder__error_info.ToString());

			//构建网格图 传递program指针
			manager_objects = new ObjectsManager(this);
			Echo(manager_objects.builder_str__init_info.ToString());

			Echo($"test: {tag__script_core_group}");

			//核心处理
			write_config_to_blocks_custom_data();

			//本脚步不自动刷新
			//if(str_error==null&&!config_script.flag__config_error)//检查是否出现错误
			//	Runtime.UpdateFrequency=UpdateFrequency.Update1;//设置执行频率 每1帧一次
		}

		//配置参数写入至方块CD中
		void write_config_to_blocks_custom_data()
		{
			string str_config = "";
			long index_turret = 0;

			//创建配置对象
			DataConfig config = new DataConfig("ANO-L");
			DataConfigSet set_config = new DataConfigSet("BLOCK CONFIGURATION");

			//添加
			config.add_config_set(set_config);

			//方块类型
			var variant__block_type = new Variant("type_block", Variant.VType.String);
			//炮塔索引
			var variant__index_turret = new Variant("index_turret", Variant.VType.Int);
			//高低部件索引
			var variant__index__elevation_componnet = new Variant("index__elevation_componnet", Variant.VType.Int);
			//额外信息(字符串)
			var variant__additional_info = new Variant("info_additional", Variant.VType.String).set_value(AdditionalInfo.None.ToString()); ;
			//额外数据(浮点数)
			var variant__additional_data = new Variant("data_additional", Variant.VType.Float).set_value(0.0d);

			set_config.add_item(variant__block_type);
			set_config.add_item(variant__index_turret);
			set_config.add_item(variant__additional_info);
			set_config.add_item(variant__additional_data);

			//全局方块

			//控制器
			variant__block_type.set_value(BlockType.Controller.ToString());
			foreach (var item in manager_objects.list_controllers)
			{
				//设置写入字符串
				config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
				config.parse_str_data();
				config.write_to_string();
			}

			//遍历所有炮塔
			foreach (var turret in manager_objects.list__turret_core_structures)
			{
				//设置数据

				//这一项无需更改
				variant__index_turret.set_value(index_turret);

				//摄像头
				variant__block_type.set_value(BlockType.Camera.ToString());
				foreach (var item in turret.list_cameras)
				{
					//设置写入字符串
					config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}

				//探测器
				variant__block_type.set_value(BlockType.Sensor.ToString());
				foreach (var item in turret.list_sensors)
				{
					//设置写入字符串
					config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}

				//武器方块
				variant__block_type.set_value(BlockType.Weapon.ToString());
				foreach (var item in turret.list_weapons)
				{
					//设置写入字符串
					config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}

				//陀螺仪
				variant__block_type.set_value(BlockType.Gyroscope.ToString());
				foreach (var item in turret.list_gyros)
				{
					//设置写入字符串
					config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}

				//定时器
				variant__block_type.set_value(BlockType.Timer.ToString());
				foreach (var item in turret.list_timers)
				{
					//设置写入字符串
					config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}

				//方向机
				variant__block_type.set_value(BlockType.AStator.ToString());
				foreach (var kvp in turret.dict__azimuth_stators_logical)
				{
					//方向机需要提供额外的安装方式信息(正向或反向或不可用)
					variant__additional_info.set_value(kvp.Value.ToString());

					//设置写入字符串
					config.set_string(() => kvp.Key.CustomData, x => { kvp.Key.CustomData = (string)x; });
					config.parse_str_data();
					config.write_to_string();
				}
				//重置
				variant__additional_info.set_value(AdditionalInfo.None.ToString());

				//子炮塔也需要区分索引
				set_config.add_item(variant__index__elevation_componnet);//炮塔索引

				long index__elevation_componnet = 0;
				//遍历高低部件
				foreach (var subturret in turret.list__elevation_component)
				{
					//设置高低部件索引
					variant__index__elevation_componnet.set_value(index__elevation_componnet++);

					//控制器
					variant__block_type.set_value(BlockType.Controller.ToString());
					foreach (var item in subturret.list_controllers)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//摄像头
					variant__block_type.set_value(BlockType.Camera.ToString());
					foreach (var item in subturret.list_cameras)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//探测器
					variant__block_type.set_value(BlockType.Sensor.ToString());
					foreach (var item in subturret.list_sensors)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//武器方块
					variant__block_type.set_value(BlockType.Weapon.ToString());
					foreach (var item in subturret.list_weapons)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//陀螺仪
					variant__block_type.set_value(BlockType.Gyroscope.ToString());
					foreach (var item in subturret.list_gyros)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//活塞
					variant__block_type.set_value(BlockType.Piston.ToString());
					foreach (var item in subturret.list_pistons)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//定时器
					variant__block_type.set_value(BlockType.Timer.ToString());
					foreach (var item in subturret.list_timers)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//其它方块
					variant__block_type.set_value(BlockType.Other.ToString());
					foreach (var item in subturret.list__other_blocks)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

					//高低机
					variant__block_type.set_value(BlockType.EStator.ToString());
					foreach (var item in subturret.list__elevation_stators)
					{
						//设置写入字符串
						config.set_string(() => item.CustomData, x => { item.CustomData = (string)x; });
						config.parse_str_data();
						config.write_to_string();
					}

				}


			}


		}

		//初始化脚本配置
		void init_script_config()
		{
			//脚本配置实例
			config_script = new DataConfig("ANO-L");
			config_script.set_string(() => Me.CustomData, (x) => Me.CustomData = x);

			config_set__script = new DataConfigSet("ANO-L CONFIGURATION");

			config_set__script.add_item((new Variant(nameof(tag__script_core_group), Variant.VType.String, () => tag__script_core_group, x => { tag__script_core_group = (string)x; })));
			config_set__script.add_item((new Variant(nameof(tag__elevation_stators), Variant.VType.String, () => tag__elevation_stators, x => { tag__elevation_stators = (string)x; })));
			config_set__script.add_item((new Variant(nameof(tag__azimuth_stators), Variant.VType.String, () => tag__azimuth_stators, x => { tag__azimuth_stators = (string)x; })));
			//添加配置项
			//config_set__script.add_config_item(nameof(tag_indicator),() => tag_indicator,x => { tag_indicator=(string)x; });
			//config_set__script.add_config_item(nameof(tag__blocking_connectivity),() => tag__blocking_connectivity,x => { tag__blocking_connectivity=(string)x; });
			//config_set__script.add_config_item(nameof(tag_fire),() => tag_fire,x => { tag_fire=(string)x; });
			//config_set__script.add_config_item(nameof(tag_hold),() => tag_hold,x => { tag_hold=(string)x; });

			config_script.add_config_set(config_set__script);

			//初始化配置
			//config_script.parse_str_data();
			// 写入
			//config_script.write_to_string();

		}

		//检查脚本配置(配置合法返回null, 否则返回错误消息)
		string check_config()
		{
			string info = "";
			return null;
		}

		#endregion

		#region 类型定义

		//方块类型
		enum BlockType
		{
			//控制器, 方向机, 高低机, 摄像头, 探测器, 武器, 自动炮塔, 陀螺仪, 活塞, 定时器, 其它.
			Controller, AStator, EStator, Camera, Sensor, Weapon, AutoTurret, Gyroscope, Piston, Timer, Other,
		}

		//补充信息
		enum AdditionalInfo
		{
			//空, 不可用, 正向, 倒置, 指示器, 阻塞, 开火, 停火
			None, Invalid, Forward, Inverted, Indicator, Blocked, Fire, Hold,
		}

		//枚举 电机安装类型
		enum MotorMountingType
		{
			//正向安装, 倒置安装, 不可用的, 未知
			Forward, Inverted, Invalid, None,
		}

		/**************************************************************************
		* 类 ElevationComponentCoreStructure
		* 高低部件核心结构
		**************************************************************************/
		class ElevationComponentCoreStructure
		{
			//列表(多个) 各类元件
			public List<IMyShipController> list_controllers = new List<IMyShipController>();
			public List<IMyCameraBlock> list_cameras = new List<IMyCameraBlock>();
			public List<IMySensorBlock> list_sensors = new List<IMySensorBlock>();
			public List<IMyUserControllableGun> list_weapons = new List<IMyUserControllableGun>();
			public List<IMyGyro> list_gyros = new List<IMyGyro>();
			public List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();
			public List<IMyTimerBlock> list_timers = new List<IMyTimerBlock>();
			public List<IMyTerminalBlock> list_indicators = new List<IMyTerminalBlock>();
			public List<IMyTerminalBlock> list__other_blocks = new List<IMyTerminalBlock>();
			//列表 高低机(转子, 活塞)
			public List<IMyMotorStator> list__elevation_stators = new List<IMyMotorStator>();

			//哈希集合 炮塔全部网格
			public HashSet<IMyCubeGrid> set_grids = new HashSet<IMyCubeGrid>();
			public bool flag_valid = true;
		}

		/**************************************************************************
		* 类 TurretCoreStructure
		* 炮塔核心结构, 将这个对象传递给类Turret的构造函数来构造一个炮塔
		* 本类实例包含了炮塔的全部网格对象, 水平电机和垂直电机对象
		**************************************************************************/
		class TurretCoreStructure
		{
			//列表(多个) 各类元件
			public List<IMyCameraBlock> list_cameras = new List<IMyCameraBlock>();
			public List<IMySensorBlock> list_sensors = new List<IMySensorBlock>();
			public List<IMyUserControllableGun> list_weapons = new List<IMyUserControllableGun>();
			public List<IMyGyro> list_gyros = new List<IMyGyro>();
			public List<IMyTimerBlock> list_timers = new List<IMyTimerBlock>();

			//字典 水平电机和逻辑安装类型
			public Dictionary<IMyMotorStator, MotorMountingType> dict__azimuth_stators_logical = new Dictionary<IMyMotorStator, MotorMountingType>();
			//列表 水平电机
			public List<IMyMotorStator> list__azimuth_stators = new List<IMyMotorStator>();
			//列表 内部子炮塔
			public List<ElevationComponentCoreStructure> list__elevation_component = new List<ElevationComponentCoreStructure>();

			//哈希集合 炮塔全部网格
			public HashSet<IMyCubeGrid> set_grids = new HashSet<IMyCubeGrid>();
			public bool flag_valid = true;
		}

		/**************************************************
		* 类 ObjectsManager
		* 对象管理器 (内部维护图结构)
		* 非泛型通用类, 为本脚本设计, 无法移植
		* 实例中包含本脚本所需的全部对象列表
		**************************************************/
		class ObjectsManager
		{
			//类 网格节点
			public class Node
			{
				public int index;//索引 List容器中的索引
				public IMyCubeGrid grid;//网格
				public bool flag_turret;//标记 炮塔网格连通性
				public bool flag__elevation_component;//标记 子炮塔网格连通性

				//列表(多个) 该网格上的各类元件
				public List<IMyShipController> list_controllers = new List<IMyShipController>();
				public List<IMyCameraBlock> list_cameras = new List<IMyCameraBlock>();
				public List<IMySensorBlock> list_sensors = new List<IMySensorBlock>();
				public List<IMyUserControllableGun> list_weapons = new List<IMyUserControllableGun>();
				public List<IMyGyro> list_gyros = new List<IMyGyro>();
				public List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();
				public List<IMyTimerBlock> list_timers = new List<IMyTimerBlock>();
				public List<IMyTerminalBlock> list_indicators = new List<IMyTerminalBlock>();
				public List<IMyTerminalBlock> list__other_blocks = new List<IMyTerminalBlock>();

				public Node(int _index, IMyCubeGrid _grid = null) { index = _index; grid = _grid; }
			}
			//类 边额外信息
			public class Edge
			{
				public bool flag__has_azimuth_motor;//标记 是否 具有水平定子
				public bool flag__has_elevation_motor;//标记 是否 具有垂直定子
				public List<IMyMotorStator> list_stators = new List<IMyMotorStator>();//列表 这条边上的定子
				public List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();//列表 这条边上的液压杆
			}
			//结构 索引
			public struct Indexes
			{
				public int index_from;
				public int index_to;

				public Indexes(int _index_from, int _index_to)
				{
					index_from = _index_from;
					index_to = _index_to;
					return;
				}
			}

			IMyBlockGroup group__script_core = null;//编组 脚本核心方块

			public List<IMyTerminalBlock> list_blocks { get; private set; } = new List<IMyTerminalBlock>();//列表 所有方块
			public List<IMyMechanicalConnectionBlock> list_mcbs { get; private set; } = new List<IMyMechanicalConnectionBlock>();//列表 全部机械连接方块(排除悬架)
			public List<IMyShipController> list_controllers { get; private set; } = new List<IMyShipController>();//列表 全部控制器
			public List<IMyMotorStator> list_stators { get; private set; } = new List<IMyMotorStator>();//列表 全部电机定子
			public List<IMyMotorStator> list__azimuth_stators { get; private set; } = new List<IMyMotorStator>();//列表 全部水平电机定子
			public List<IMyMotorStator> list__elevation_stators { get; private set; } = new List<IMyMotorStator>();//列表 全部垂直电机定子
			public List<IMyCameraBlock> list_cameras { get; private set; } = new List<IMyCameraBlock>();//列表 全部摄像头
			public List<IMySensorBlock> list_sensors { get; private set; } = new List<IMySensorBlock>();//列表 全部探测器
			public List<IMyUserControllableGun> list_weapons { get; private set; } = new List<IMyUserControllableGun>();//列表 全部武器
			public List<IMyLargeTurretBase> list__auto_turrets { get; private set; } = new List<IMyLargeTurretBase>();//列表 全部自动炮塔
			public List<IMyGyro> list_gyros { get; private set; } = new List<IMyGyro>();//列表 陀螺仪
			public List<IMyPistonBase> list_pistons { get; private set; } = new List<IMyPistonBase>();//列表 活塞
			public List<IMyTimerBlock> list_timers { get; private set; } = new List<IMyTimerBlock>();//列表 定时器
			public List<IMyTextPanel> list_displayers { get; private set; } = new List<IMyTextPanel>();//列表 显示器
			public List<IMyTerminalBlock> list_indicators { get; private set; } = new List<IMyTerminalBlock>();//列表 指示器
			public HashSet<IMyTerminalBlock> set_indicators { get; private set; } = new HashSet<IMyTerminalBlock>();//集合 指示器
			public List<IMyTerminalBlock> list__other_blocks { get; private set; } = null;//列表 其它方块
			public HashSet<long> set__self_ids { get; private set; } = new HashSet<long>();//哈希表 自身网格的ID
			public List<Node> list_nodes { get; private set; } = new List<Node>();//列表 图的节点
			Dictionary<IMyCubeGrid, int> dict__grids_index = new Dictionary<IMyCubeGrid, int>();//字典 根据网格快速检索节点索引
			Dictionary<IMyMotorStator, Indexes> dict__edge_index_of_motor = new Dictionary<IMyMotorStator, Indexes>();//字典 根据电机查找它处在的边的索引

			//列表 炮塔核心结构列表 执行构造函数之后会生成此列表
			public List<TurretCoreStructure> list__turret_core_structures { get; private set; } = new List<TurretCoreStructure>();
			public List<ElevationComponentCoreStructure> list__elevation_component_core_structures { get; private set; } = new List<ElevationComponentCoreStructure>();

			//字典 存储所有边, 考虑到用邻接矩阵来表示稀疏图, 时间空间浪费过大, 因此转而使用邻接表
			List<Dictionary<int, Edge>> list_adjacency = new List<Dictionary<int, Edge>>();

			Program p = null;
			//字符串构建器 初始化信息
			public StringBuilder builder_str__init_info { get; private set; } = new StringBuilder();

			//构造函数
			public ObjectsManager(Program _program)
			{
				//成员赋值
				p = _program;
				//获取脚本核心编组
				group__script_core = p.GridTerminalSystem.GetBlockGroupWithName(p.tag__script_core_group);
				if (group__script_core == null) return;
				group__script_core.GetBlocks(list_blocks);
				group__script_core.GetBlocksOfType(list_controllers);
				group__script_core.GetBlocksOfType(list_stators);
				group__script_core.GetBlocksOfType(list_cameras);
				group__script_core.GetBlocksOfType(list_sensors);
				group__script_core.GetBlocksOfType(list_weapons);
				group__script_core.GetBlocksOfType(list__auto_turrets);
				group__script_core.GetBlocksOfType(list_gyros);
				group__script_core.GetBlocksOfType(list_pistons);
				group__script_core.GetBlocksOfType(list_timers);
				group__script_core.GetBlocksOfType(list_displayers);
				//foreach(var i in list_blocks) if(i.CustomData.StartsWith(p.tag_indicator)) list_indicators.Add(i);

				//查找其它方块
				HashSet<IMyTerminalBlock> set_tmp, set_total = new HashSet<IMyTerminalBlock>(list_blocks);
				set_tmp = new HashSet<IMyTerminalBlock>(list_cameras);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_sensors);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_controllers);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_stators);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_weapons);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list__auto_turrets);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_gyros);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_pistons);
				set_total.ExceptWith(set_tmp);//差集
				set_tmp = new HashSet<IMyTerminalBlock>(list_timers);
				set_total.ExceptWith(set_tmp);//差集
				list__other_blocks = set_total.ToList();//转换为列表

				List<IMyMechanicalConnectionBlock> list_temp = new List<IMyMechanicalConnectionBlock>();
				p.GridTerminalSystem.GetBlocksOfType(list_temp);//获取全部机械连接方块(电机, 铰链, 活塞, 悬架)
				foreach (var item in list_temp) if (!(item is IMyMotorSuspension)) list_mcbs.Add(item);

				int index = 0;
				//获取全部网格 构建节点列表和节点索引
				foreach (var item in list_mcbs)
				{
					if (!dict__grids_index.ContainsKey(item.CubeGrid))
					{
						list_nodes.Add(new Node(index, item.CubeGrid));
						dict__grids_index[item.CubeGrid] = index++; set__self_ids.Add(item.CubeGrid.EntityId);
						builder_str__init_info.Append($"<info> node {index - 1} {item.CubeGrid.CustomName}\n");
					}
					if (item.TopGrid != null && !dict__grids_index.ContainsKey(item.TopGrid))
					{
						list_nodes.Add(new Node(index, item.TopGrid));
						dict__grids_index[item.TopGrid] = index++; set__self_ids.Add(item.TopGrid.EntityId);
						builder_str__init_info.Append($"<info> node {index - 1} {item.TopGrid.CustomName}\n");
					}
				}

				//区分水平电机定子和垂直电机定子, 添加到对应列表中, 未标注的定子将被忽略
				foreach (var item in list_stators)
					if (item.CustomName.Contains(p.tag__azimuth_stators))
						list__azimuth_stators.Add(item);//添加到列表
					else if (item.CustomName.Contains(p.tag__elevation_stators))
						list__elevation_stators.Add(item);//添加到列表

				//构建邻接表 不再使用邻接矩阵, 邻接表无需初始化
				for (int i = 0; i < list_nodes.Count; ++i)
					list_adjacency.Add(new Dictionary<int, Edge>());//创建每个节点的临边字典

				//向邻接表中添加边
				foreach (var item in list_mcbs)
				{
					if (item.CustomData.Contains(p.tag__blocking_connectivity) || item.TopGrid == null)
						continue;//跳过被用户注册为阻塞的, 或无头部网格的机械连接方块
					int index_from = dict__grids_index[item.CubeGrid];
					int index_to = dict__grids_index[item.TopGrid];
					if (!list_adjacency[index_to].ContainsKey(index_from))
					{
						Edge edge = new Edge();
						list_adjacency[index_from][index_to] = edge;
						list_adjacency[index_to][index_from] = edge;
					}
				}

				//注册
				foreach (var item in list__azimuth_stators)
				{
					if (item.TopGrid == null)
						continue;
					int index_from = dict__grids_index[item.CubeGrid];
					int index_to = dict__grids_index[item.TopGrid];
					list_adjacency[index_from][index_to].flag__has_azimuth_motor = true;
					//添加到对应边的列表中
					list_adjacency[index_from][index_to].list_stators.Add(item);
					//在索引字典中注册
					dict__edge_index_of_motor[item] = new Indexes(index_from, index_to);
				}
				foreach (var item in list__elevation_stators)
				{
					if (item.TopGrid == null) continue;
					int index_from = dict__grids_index[item.CubeGrid];
					int index_to = dict__grids_index[item.TopGrid];
					list_adjacency[index_from][index_to].flag__has_elevation_motor = true;
					//添加到对应边的列表中
					list_adjacency[index_from][index_to].list_stators.Add(item);
					//在索引字典中注册
					dict__edge_index_of_motor[item] = new Indexes(index_from, index_to);
				}
				foreach (var item in list_pistons)
				{
					if (item.TopGrid == null) continue;
					int index_from = dict__grids_index[item.CubeGrid];
					int index_to = dict__grids_index[item.TopGrid];
					//添加到对应边的列表中
					list_adjacency[index_from][index_to].list_pistons.Add(item);
				}

				//节点注册元件
				foreach (var item in list_controllers) list_nodes[dict__grids_index[item.CubeGrid]].list_controllers.Add(item);
				foreach (var item in list_cameras) list_nodes[dict__grids_index[item.CubeGrid]].list_cameras.Add(item);
				foreach (var item in list_sensors) list_nodes[dict__grids_index[item.CubeGrid]].list_sensors.Add(item);
				foreach (var item in list_weapons) list_nodes[dict__grids_index[item.CubeGrid]].list_weapons.Add(item);
				foreach (var item in list_gyros) list_nodes[dict__grids_index[item.CubeGrid]].list_gyros.Add(item);
				foreach (var item in list_pistons) list_nodes[dict__grids_index[item.CubeGrid]].list_pistons.Add(item);
				foreach (var item in list_timers) list_nodes[dict__grids_index[item.CubeGrid]].list_timers.Add(item);
				foreach (var item in list__other_blocks) list_nodes[dict__grids_index[item.CubeGrid]].list__other_blocks.Add(item);

				//BFS搜索
				foreach (var item in list__elevation_stators)
				{
					if (item.TopGrid == null)
						continue;
					int index_from = dict__grids_index[item.CubeGrid];
					if (list_nodes[index_from].flag_turret)
						continue;//被搜索过了
					bfs_turret(index_from);//广度优先搜索 执行一次BFS同时会生成一个炮塔核心结构对象
				}

				foreach (var item in list_cameras)//BFS搜索
				{
					index = dict__grids_index[item.CubeGrid];
					bfs_elevation_component(index);
				}
				foreach (var item in list_weapons)//BFS搜索
				{
					index = dict__grids_index[item.CubeGrid];
					bfs_elevation_component(index);
				}
				foreach (var item in list_controllers)//BFS搜索
				{
					index = dict__grids_index[item.CubeGrid];
					bfs_elevation_component(index);
				}
				foreach (var item in list__other_blocks)//BFS搜索
				{
					index = dict__grids_index[item.CubeGrid];
					bfs_elevation_component(index);
				}

				//搜索液压杆
				foreach (var item in list__elevation_component_core_structures)
				{

				}

				generate_init_info();
			}

			private void bfs_turret(int index_start)
			{
				Queue queue = new Queue();//队列 索引队列
				queue.Enqueue(index_start);//起点入队列
				list_nodes[index_start].flag_turret = true;
				TurretCoreStructure struct__turret_core = new TurretCoreStructure();//炮塔核心结构
				builder_str__init_info.Append($"<info> bfs_turret(): start {index_start} {list_nodes[index_start].grid.CustomName}\n");

				while (queue.Count != 0)//主循环
				{
					int index_crt = (int)queue.Dequeue();//索引 当前索引
					Node node_crt = list_nodes[index_crt];//当前节点
					struct__turret_core.set_grids.Add(node_crt.grid);
					builder_str__init_info.Append($"<info> -> {index_crt} {list_nodes[index_crt].grid.CustomName}\n");

					struct__turret_core.list_cameras.AddRange(node_crt.list_cameras);
					struct__turret_core.list_sensors.AddRange(node_crt.list_sensors);
					struct__turret_core.list_weapons.AddRange(node_crt.list_weapons);
					struct__turret_core.list_gyros.AddRange(node_crt.list_gyros);
					struct__turret_core.list_timers.AddRange(node_crt.list_timers);

					//遍历出边 找到没有阻塞性, 并且抵达的网格没有被标记过的的出边
					for (int index_next = 0; index_next < list_nodes.Count; ++index_next)
					{
						if (!list_adjacency[index_crt].ContainsKey(index_next))
							continue;//不是出边就跳过
						Node node_next = list_nodes[index_next];//下一个节点
						Edge edge = list_adjacency[index_crt][index_next];//边

						if (edge.flag__has_azimuth_motor)
						{
							//这条边上存在水平电机, 把它们添加到核心结构对象中
							struct__turret_core.list__azimuth_stators.AddRange(edge.list_stators);
							continue;//跳过
						}
						//检查是否已经进入过队列
						if (node_next.flag_turret == false) { node_next.flag_turret = true; queue.Enqueue(index_next); }
					}
				}//主循环结束

				//检查水平转子的逻辑安装方式
				foreach (var item in struct__turret_core.list__azimuth_stators)
				{
					//获取这个水平电机所在边在矩阵中的索引
					Indexes indexes = dict__edge_index_of_motor[item];
					//获取这条边两边的网格的连通性
					bool flag_0 = list_nodes[indexes.index_from].flag_turret;
					bool flag_1 = list_nodes[indexes.index_to].flag_turret;

					if (flag_0 == flag_1)//都连通或者都不连通
					{
						struct__turret_core.dict__azimuth_stators_logical[item] = MotorMountingType.Invalid;//不可用
						struct__turret_core.flag_valid = false;//设为不可用
					}
					else
						if (list_nodes[dict__grids_index[item.TopGrid]].flag_turret)
						struct__turret_core.dict__azimuth_stators_logical[item] = MotorMountingType.Forward;
					else
						struct__turret_core.dict__azimuth_stators_logical[item] = MotorMountingType.Inverted;
				}
				list__turret_core_structures.Add(struct__turret_core);//添加到列表
				builder_str__init_info.Append($"<info> bfs_turret(): end\n\n");
			}

			private void bfs_elevation_component(int index_start)
			{
				if (list_nodes[index_start].flag__elevation_component)
					return;//被搜索过了

				Queue queue = new Queue();//队列 索引队列
				queue.Enqueue(index_start);//起点入队列
				list_nodes[index_start].flag__elevation_component = true;
				ElevationComponentCoreStructure s__st_c = new ElevationComponentCoreStructure();//炮塔核心结构

				builder_str__init_info.Append($"<info> bfs_elevation_component(): start {index_start} {list_nodes[index_start].grid.CustomName}\n");
				bool flag_h = true;
				while (queue.Count != 0)//主循环
				{
					int i_crt = (int)queue.Dequeue();//索引 当前索引
					Node n_crt = list_nodes[i_crt];//当前节点
					s__st_c.set_grids.Add(n_crt.grid);
					builder_str__init_info.Append($"<info> -> {i_crt} {list_nodes[i_crt].grid.CustomName}\n");

					s__st_c.list_controllers.AddRange(n_crt.list_controllers);
					s__st_c.list_cameras.AddRange(n_crt.list_cameras);
					s__st_c.list_sensors.AddRange(n_crt.list_sensors);
					s__st_c.list_weapons.AddRange(n_crt.list_weapons);
					s__st_c.list_gyros.AddRange(n_crt.list_gyros);
					s__st_c.list_timers.AddRange(n_crt.list_timers);
					s__st_c.list_indicators.AddRange(n_crt.list_indicators);
					s__st_c.list__other_blocks.AddRange(n_crt.list__other_blocks);

					//if(n_crt.grid.CustomName.Contains(p.tag__ele_pistons_gird))
					//	search_pistons(i_crt,s__st_c);//搜索活塞

					//遍历出边 找到没有阻塞性, 并且抵达的网格没有被标记过的的出边
					foreach (var index_next in new List<int>(list_adjacency[i_crt].Keys))
					{
						Node node_next = list_nodes[index_next];//下一个节点
						Edge edge = list_adjacency[i_crt][index_next];//边
						if (edge.flag__has_azimuth_motor) { flag_h = false; break; }//发现水平电机退出
						if (edge.flag__has_elevation_motor)
						{
							//这条边上存在高低机, 把它们添加到核心结构对象中
							s__st_c.list__elevation_stators.AddRange(edge.list_stators);
							continue;//跳过
						}
						//检查是否已经进入过队列
						if (node_next.flag__elevation_component == false)
						{ node_next.flag__elevation_component = true; queue.Enqueue(index_next); }

					}
				}//主循环结束
				if (flag_h)
					foreach (var item in list__turret_core_structures)//添加到炮塔核心结构
						if (s__st_c.set_grids.IsSubsetOf(item.set_grids))
							item.list__elevation_component.Add(s__st_c);
				builder_str__init_info.Append($"<info> bfs_elevation_component(): end\n\n");
			}
			//搜索活塞
			private void search_pistons(int index_start, ElevationComponentCoreStructure core)
			{
				builder_str__init_info.Append($"<info> search_pistons(): start {index_start} {list_nodes[index_start].grid.CustomName}\n");
				Queue queue = new Queue();//队列 索引队列
				queue.Enqueue(index_start);//起点入队列
				while (queue.Count != 0)//主循环
				{
					int index_crt = (int)queue.Dequeue();//索引 当前索引
					Node node_crt = list_nodes[index_crt];//当前节点
					core.set_grids.Add(node_crt.grid);
					core.list_pistons.AddRange(node_crt.list_pistons);//追加
					builder_str__init_info.Append($"<info> -> {index_crt} {list_nodes[index_crt].grid.CustomName}\n");
					//遍历出边 找到没有阻塞性, 抵达的网格没有被标记过且具有活塞的出边
					for (int index_next = 0; index_next < list_nodes.Count; ++index_next)
					{
						Node node_next = list_nodes[index_next];//下一个节点
						if (!list_adjacency[index_crt].ContainsKey(index_next)/*||!node_next.grid.CustomName.Contains(p.tag__ele_pistons_gird)*/)
							continue;//不是出边或不是特定网格就跳过
						if (node_next.flag__elevation_component == false) { node_next.flag__elevation_component = true; queue.Enqueue(index_next); }
					}
				}
				builder_str__init_info.Append($"<info> search_pistons(): end\n\n");
			}
			//生成初始化信息
			private void generate_init_info()
			{
				builder_str__init_info.Append(
					$"<info> list__turret_core_structures.Count = {list__turret_core_structures.Count}\n"
					+ $"<info> list_blocks.Count = {list_blocks.Count}\n"
					+ $"<info> list_cameras.Count = {list_cameras.Count}\n"
					+ $"<info> list_sensors.Count = {list_sensors.Count}\n"
					+ $"<info> list_controllers.Count = {list_controllers.Count}\n"
					+ $"<info> list_stators.Count = {list_stators.Count}\n"
					+ $"<info> list_weapons.Count = {list_weapons.Count}\n"
					+ $"<info> list__auto_turrets.Count = {list__auto_turrets.Count}\n"
					+ $"<info> list_gyroscopes.Count = {list_gyros.Count}\n"
					+ $"<info> list_timers.Count = {list_timers.Count}\n"
					+ $"<info> list_displayers.Count = {list_displayers.Count}\n"
					+ $"<info> list__other_blocks.Count = {list__other_blocks.Count}\n");
			}
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
																   //bool? v_b; long? v_l; double? v_d; string v_s = null; Vector3D? v_v3d;

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