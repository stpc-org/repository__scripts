/********************************************************************************************************************************************************************************************************          
* 
* ### ANOFCS's Not Only Fire Control System  ###
* ### ANOFCS | "匿名者" 火控系统脚本 ------- ###
* ### Version 0.0.0 | by SiriusZ-BOTTLE ---- ###
* ### STPC旗下SCP脚本工作室开发 欢迎加入STPC ###
* ### STPC主群群号:320461590 我们欢迎新朋友- ###
* 
* 
* ============================== 脚本功能 ==============================
* 
* [0] 全自动多炮塔识别, 多炮塔控制, 支持以任意方式安装在任意位置和方向的炮塔.
*     此脚本智能识别以任意方向安装的多个水平或垂直炮塔转子!
*     它们甚至可以位于不同的网格上! 并且你无需关心转子的正反, 全自动识别!!
* [1] 脚本支持有限角度炮塔, 可自动从另一侧进行旋转(参考战舰世界战列舰主炮塔换侧旋转)
*     (有限角炮塔即炮塔不能360转圈的炮塔, 如二战战列舰主炮塔, 坦克炮塔则通常为无限角炮塔)
* 
* [1] 支持第三人称手动操控多炮塔, 可以配置默认是否开启此功能
* [2] 进入摄像头操控对应炮塔, 可通过命令切换同步或异步模式(操控全部炮塔或单个炮塔), 
*     此时无论哪个炮塔被操控, 被脚本注册的全部摄像头轮流进行激光扫描, 搜寻目标.
* 
* [3] 摄像头发现目标之后自动尝试保持锁定, 可通过命令取消锁定,
*     取消锁定后一定时间内不再锁定相同目标, 支持使用载具上的固定摄像头来辅助锁定目标
* 
* [4] 设置武器炮弹飞行速度和加速度等各种信息后脚本提供预判功能, 
*     此外脚本支持在重力环境下预判受到重力影响的炮弹(如活塞火炮炮弹)
*     
* [5] 支持将原生自动炮塔的目标作为脚本锁定目标, 出现多目标时按照规则进行分配
* [6] 炮塔相对方位指示器, 在LCD上绘制炮塔相对于载具的方位
* [7] 雷达画面显示, 在LCD上绘制目标的相对位置, 同时在LCD上标出友军位置(需具有相同脚本)
* [8] 智能防误伤, 射界内出现同级网格时自动禁用武器, 并且脚本不锁定同级网格
* [9] 多炮塔异步自动归位(即, 炮塔初始位置不同, 可配置炮塔初始位置)
* [10] 脚本开放定时器接口, 可适用于自定义的武器(如活塞炮), 或实现特殊效果
* [11] 与未来计划中的导弹脚本兼容
* [12] 支持炮塔大角度旋转稳定器, 专业防手抖!
* 
* 
* ============================== 工作机制 ==============================
* 
* ========== 炮塔旋转控制 ==========
* 
* [0] 炮塔旋转控制有三种工作模式
*     [0] 全局坐标驱动      即炮塔旋转, 最终摄像头指向特定坐标
*         坐标驱动实现让所有的炮塔指向特定目标, 也就是说这些炮塔的指向射线最终交汇于某个点
*     [1] 全局方向驱动      即炮塔旋转, 最终摄像头的方向向量与目标方向向量平行
*         方向驱动实现让所有的炮塔的指向射线互相平行, 当没有目标, 但玩家操控全部炮塔时采用这种模式
*     [2] 局部方向驱动      即炮塔旋转, 最终水平转子或垂直转子达到特定角度
*         炮塔归位时使用该模式
* 
* [1] 全局坐标驱动模式误差角度计算
*     [0] 获取转子的旋转平面法向量(转子的 up方向向量) N, 主摄像头的朝向向量 DC0, 
*         主摄像头的空间位置向量 PC, 以及目标坐标向量 T
*     [1] 令向量 E = T - PC, 得到方位误差向量(目标向量)
*     [2] 通过向量 N, 分别计算向量 E 和向量 DC0 在转子旋转面上的投影向量, 得到 PE 和 PDC0 向量
*     [3] 令夹角 θ = <PE, PDC0>, 该角即为误差角, 令向量 X = PE x PDC0 (外积)
*     [4] 若 X N 方向相同, 则转子需要顺时针旋转(从up方向看), 反之需要逆时针旋转
* 
* [2] 全局方向驱动模式误差角度计算
*     [0] 与坐标驱动模式类似, 可以省略前几步, 直接使用全局方向进行投影
* 
* [3] 局部方向驱动模式误差角度计算
*     [0] 直接获取转子数据即可, 不再赘述
* 
* [4] 根据误差角度, 执行控制操作
*     [0] 常见控制算法(如PID等)
*     [1] 锁死机制(限位等)
* 
* 
********************************************************************************************************************************************************************************************************/

#region using 声明

//用于IDE开发的 using 声明, 脚本运行时请注释掉这一段代码
//using Sandbox.ModAPI.Ingame;
//using SpaceEngineers.Game.ModAPI.Ingame;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using VRage.Game.ModAPI.Ingame;
//using VRageMath;

#endregion

#region 脚本字段

//数组 运行时字符显示
string[] array__runtime_chars = new string[] { "", "", "R", "RU", "RUN", "RUNN", "RUNNI", "RUNNIN", "RUNNING", "RUNNING", };

//标签 脚本核心编组
string tag__script_core_group = "Turret";
//标签 垂直转子(俯仰转子)
string tag__elevation_stators = "_V";
//标签 水平转子(偏航转子)
string tag__azimuth_stators = "_H";
//标签 高低机活塞网格
string tag__elevation_pistons_gird = "ElevationPistonGrid";

//标记 是否 自动检测(自动检查炮塔完整性)
bool flag__auto_check = true;
//标记 是否 默认开启自动射击
bool flag__auto_fire_by_default = true;
//标记 是否 在全局操控模式下扫描
bool flag__scan_under_global_control_mode = true;
//标记 是否 单独计算每个电机的数据
//bool flag__cal_data_on_each_motor_separately = false;
//标记 是否 默认开启全局操控模式(此项针对飞船考虑)
bool flag__enable_global_control_mode_by_default = true;
//标记 是否 锁定自动炮塔的目标
bool flag__lock_target_of_auto_turret = true;
//标记 是否 当没有用户时锁定自动炮塔的目标
bool flag__lock_target_of_auto_turret_when_no_user = true;
//标记 是否 当用户离开时归位炮塔
bool flag__reset_turret_angle_when_user_leave = true;
//标记 是否 当用户进入时重置全局朝向
bool flag__reset_global_orientation_when_user_enter = false;
//标记 是否 默认在单一操控模式下进行异步操控(一般设为true)
bool flag__asynchronously_control_by_default = true;
//标记 是否 默认锁定之后玩家失去控制(单一控制模式)
bool flag__user_lose_turret_control_after_locking_by_default = true;
//标记 是否 启用水平偏转稳定器
bool flag__enable_azimuth_deflection_stabilizer = true;
//标记 是否 启用垂直偏转稳定器
bool flag__enable_elevation_deflection_stabilizer = true;
//标记 是否 启用操控平面防跨越
bool flag__enable_control_plane_crossing_prevention = true;
//标记 是否 启用非数投射
bool flag__enable_n_raycast = false;
//标记 是否 忽略玩家
bool flag__ignore_players = false;
//标记 是否 忽略火箭弹
bool flag__ignore_rockets = false;
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


//周期 自动检查(默认每秒检查一次)
int period__auto_check = 60;
//周期 更新输出(默认每秒2次)
int period__update_output = 30;

//角度 转轴间最大容忍角度 区间[0.1,60.0]
double degree__max_angle_tolerance_between_axis = 3.0;
//比率 鼠标水平敏感度
double ratio__mouse_azimuth_sensitivity = 0.2;
//比率 鼠标垂直敏感度
double ratio__mouse_elevation_sensitivity = 0.2;
//距离 全局操控模式下的扫描距离
double distance__scan_under_global_control_mode = 1000.0;
//距离 单一操控模式下的扫描距离
double distance__scan_under_single_control_mode = 2000.0;
//系数 PID算法三项基准系数(全局)
PIDCore.CoeffVector vector_coeff = new PIDCore.CoeffVector();

//以上是脚本配置字段

//网格图结构
GridsGraph graph_grids;

//列表 炮塔
List<Turret> list_turrets = new List<Turret>();
//列表 显示单元
List<DisplayUnit> list__display_units = new List<DisplayUnit>();

//炮塔 单一控制模式下处于控制的炮塔
Turret turret__under_control;
//模式 用户操控模式
ControlMode mode_control = ControlMode.GlobalControlMode;
//模式 用户操控模式(上一个, 用来指示模式切换)
ControlMode mode__control_prev = ControlMode.None;

//计数 脚本触发次数(工具栏手动触发次数)
long count_trigger = 0;
//计数 更新
long count_update = 0;
//索引 当前字符图案
long index__crt_char_pattern = 0;
//次数 距离下一次 字符图案更新
long times__before_next_char_pattern_update = 0;
//次数 距离下一次 更新输出
long times__before_next_output_update = 0;
//次数 距离下一次 扫描
long times__before_next_scan = 0;
//计数 固定摄像机
long count__fixed_cameras = 0;
//周期 扫描周期(全局模式)
long period__scan_gcm = 0;
//周期 扫描周期(单一模式)
long period__scan_scm = 0;
//增量 每一帧的增量
double increment__per_frame = 0;

IMyShipController controller_main;//当前控制中的控制器
Vector3D position;//当前位置
//向量3D 脚本全局朝向
Vector3D vector__global_orientation = new Vector3D();
//向量2 旋转指示器
Vector2 vector__rotation_indicator = new Vector2();
//向量2 旋转指示器积分
Vector2 vector__rotation_indicator_integral = new Vector2();

//实体信息 用户目标实体信息
MyDetectedEntityInfo entity_info__custom_target;
//实体信息 上一个目标
MyDetectedEntityInfo entity_info__custom_target_prev;

//列表 自动炮塔的目标实体信息
List<MyDetectedEntityInfo> list__entity_infos_of_auto_turret = new List<MyDetectedEntityInfo>();

//字符串构建器 默认信息
StringBuilder string_builder__default_info = new StringBuilder();
//字符串构建器 测试信息
StringBuilder string_builder__test_info = new StringBuilder();
//脚本配置
CustomDataConfig config_script;
//脚本主配置集合
CustomDataConfigSet config_set__script;

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
	return;
}

/***************************************************************************************************
* 入口函数 main()
***************************************************************************************************/
void Main(string str_arg, UpdateType type_update)
{
	if (type_update == UpdateType.Trigger)
	{
		//工具栏带参数执行
		//检查命令
		switch (str_arg)
		{

		}
		++count_trigger;
	}
	else if (type_update == UpdateType.Update1)
	{
		update_script();
	}
}

#endregion

#region 功能函数

//脚本更新
void update_script()
{
	position = Me.GetPosition();
	//检查用户操控模式
	controller_main = null;
	mode__control_prev = mode_control;//记录上一次的控制模式
	foreach (var item in graph_grids.list_controllers)
		if (item.IsUnderControl)//找到第一个处于控制的控制器
		{
			controller_main = item;
			break;
		}
	if (controller_main == null)//无人控制
		mode_control = ControlMode.None;
	else
	{
		mode_control = ControlMode.GlobalControlMode;
		foreach (var item in list_turrets)
			if (item.check_if_under_control())
			{
				mode_control = ControlMode.SingleControlMode;
				turret__under_control = item;//当前受控制的炮塔
				break;
			}
		vector__rotation_indicator = controller_main.RotationIndicator;//获取旋转指示器
		float tmp = -vector__rotation_indicator.X;
		vector__rotation_indicator.X = vector__rotation_indicator.Y * (float)ratio__mouse_azimuth_sensitivity;
		vector__rotation_indicator.Y = tmp * (float)ratio__mouse_elevation_sensitivity;
		vector__rotation_indicator /= 200.0f;

		if (flag__enable_azimuth_deflection_stabilizer && Math.Abs(vector__rotation_indicator.X) > Math.Abs(vector__rotation_indicator.Y))
			vector__rotation_indicator.Y = vector__rotation_indicator.Y * Math.Abs(vector__rotation_indicator.Y / vector__rotation_indicator.X);
		else if (flag__enable_elevation_deflection_stabilizer && Math.Abs(vector__rotation_indicator.Y) > Math.Abs(vector__rotation_indicator.X))
			vector__rotation_indicator.X = vector__rotation_indicator.X * Math.Abs(vector__rotation_indicator.X / vector__rotation_indicator.Y);

		var quaternion_azimuth = Quaternion.CreateFromAxisAngle(controller_main.WorldMatrix.Down, vector__rotation_indicator.X);
		Vector3D.Transform(ref vector__global_orientation, ref quaternion_azimuth, out vector__global_orientation);
		var quaternion_elevation = Quaternion.CreateFromAxisAngle
			(Vector3D.Normalize(Vector3D.Cross(controller_main.WorldMatrix.Down, vector__global_orientation)), vector__rotation_indicator.Y);

		double angle = (180 / Math.PI) * Math.Acos//计算向量的夹角(转为角度制)
				(Vector3D.Dot(controller_main.WorldMatrix.Up, vector__global_orientation) /
				(controller_main.WorldMatrix.Up.Length() * vector__global_orientation.Length()));
		if (!flag__enable_control_plane_crossing_prevention || (angle < 1 ? (vector__rotation_indicator.Y < 0) : (angle <= 179.0 || (vector__rotation_indicator.Y > 0))))
			Vector3D.Transform(ref vector__global_orientation, ref quaternion_elevation, out vector__global_orientation);
		vector__global_orientation.Normalize();//标准化全局向量
		//vector__rotation_indicator_integral += vector__rotation_indicator;//累计值
	}

	switch (mode_control)
	{
	case ControlMode.GlobalControlMode://全局操控模式
		{
			if (mode__control_prev == ControlMode.None) reset_global_orientation();
			if (flag__scan_under_global_control_mode)
				scan();
			foreach (var item in list_turrets)//设置目标方向
				item.set_target_orientation(vector__global_orientation);
		}
		break;
	case ControlMode.SingleControlMode://单一操控模式
		{
			foreach (var item in list_turrets)//设置目标方向
				item.set_target_orientation(vector__global_orientation);
			scan(false);
		}
		break;
	case ControlMode.AutomaticControlMode://自动控制模式
		{

		}
		break;
	case ControlMode.None:
		{
			if (flag__reset_turret_angle_when_user_leave && mode__control_prev != ControlMode.None)
			{
				foreach (var item in list_turrets) item.reset();
			}
		}
		break;
	}

	try {
		//更新所有炮塔
		foreach (var item in list_turrets)
			item.update();
	}catch (Exception e) { Me.CustomData = e.ToString(); }
	
	//信息显示
	display_info();
	//更新次数+1
	++count_update;
	return;
}
//扫描(搜寻目标)
void scan(bool global = true)
{
	if (flag__enable_n_raycast) { scan_n(global); return; }
	if (times__before_next_scan == 0)
	{
		times__before_next_scan = global ? period__scan_gcm : period__scan_scm;
		Vector3D tgt = controller_main.GetPosition() + vector__global_orientation *
		(global ? distance__scan_under_global_control_mode : distance__scan_under_single_control_mode);
		foreach (var item in graph_grids.list_cameras)
			if (item.CanScan(tgt)) { entity_info__custom_target = item.Raycast(tgt); break; }
	}
	--times__before_next_scan;
}

void scan_n(bool global = true)
{
	if (graph_grids.list_cameras.Count == 0) return;
	var cam = graph_grids.list_cameras.First(); float yaw, pitch;
	Vector3D tgt = controller_main.GetPosition() + vector__global_orientation *
		(global ? distance__scan_under_global_control_mode : distance__scan_under_single_control_mode);
	global_to_yaw_pitch(out yaw, out pitch, graph_grids.list_cameras.First().WorldMatrix, tgt - cam.GetPosition());
	cam.Raycast(distance__scan_under_single_control_mode, pitch, yaw);
}

//全局转 yaw pitch
static void global_to_yaw_pitch(out float yaw, out float pitch, MatrixD matrix, Vector3D tgt)
{
	Vector3D l = matrix.Left, u = matrix.Up, f = matrix.Forward;
	var p2l = Vector3D.ProjectOnPlane(ref tgt, ref l);//计算投影
	var p2u = Vector3D.ProjectOnPlane(ref tgt, ref u);//计算投影
	double num = Vector3D.Dot(p2l, f) / (p2l.Length() * f.Length());
	if (num > 1.0) num = 1.0; if (num < -1.0) num = -1.0;
	pitch = (float)(Math.Acos(num) * 180 / Math.PI); if (Vector3D.Dot(u, p2l) < 0) pitch = -pitch;
	num = Vector3D.Dot(p2u, f) / (p2u.Length() * f.Length());
	if (num > 1.0) num = 1.0; if (num < -1.0) num = -1.0;
	yaw = (float)(Math.Acos(num) * 180 / Math.PI); if (Vector3D.Dot(l, p2u) > 0) yaw = -yaw;
}

//输出信息 输出信息到编程块终端和LCD
void display_info()
{
	if (times__before_next_output_update != 0)
	{
		--times__before_next_output_update; return;
	}
	else
		times__before_next_output_update = period__update_output;
	--times__before_next_output_update;

	//清空
	string_builder__default_info.Clear();
	string_builder__default_info.Append(
		"<script> ANOFCS V0.0.0 " + array__runtime_chars[index__crt_char_pattern]
		+ "\n<count_update> " + count_update
		//+ "\n<count_trigger> " + count_trigger
		+ "\n<control_mode> " + mode_control
		+ "\n<target_name> " + entity_info__custom_target.Name
		+ "\n<target_type> " + entity_info__custom_target.Type
		+ "\n<target_velocity> " + entity_info__custom_target.Velocity
		+ "\n<target_distance> " + (entity_info__custom_target.EntityId != 0 ? entity_info__custom_target.HitPosition.Value : position - position).Length()
		//+ "\n<period__scan_gcm> " + period__scan_gcm
		//+ "\n<period__scan_scm> " + period__scan_scm
		+ "\n<indicator_rotation> " + vector__rotation_indicator.X.ToString("f2") + " , " + vector__rotation_indicator.Y.ToString("f2")
		+ "\n<orientation_global> " + vector__global_orientation.X.ToString("f2") + " , " + vector__global_orientation.Y.ToString("f2") + " , " + vector__global_orientation.Z.ToString("f2")
		+ "\n<count_all_blocks> " + graph_grids.list_blocks.Count
		+ "\n<count_controllers> " + graph_grids.list_controllers.Count
		+ "\n<count_total_stators> " + graph_grids.list_stators.Count
		+ "\n<count_azimuth_stators> " + graph_grids.list__azimuth_stators.Count
		+ "\n<count_elevation_stators> " + graph_grids.list__elevation_stators.Count
		+ "\n<count_cameras> " + graph_grids.list_cameras.Count
		+ "\n<count__fixed_cameras> " + count__fixed_cameras
		+ "\n<count_sensors> " + graph_grids.list_sensors.Count
		+ "\n<count_weapon_blocks> " + graph_grids.list_weapons.Count
		+ "\n<count_auto_turrets> " + graph_grids.list__auto_turrets.Count
		+ "\n<count__total_grids> " + graph_grids.list_nodes.Count
		+ "\n<count_turret> total " + list_turrets.Count + "\n"
		);
	Echo(string_builder__default_info.ToString());

	//更新动态字符图案
	if (times__before_next_char_pattern_update == 0)
	{
		times__before_next_char_pattern_update = 1;
		++index__crt_char_pattern;
		if (index__crt_char_pattern >= array__runtime_chars.Length)
			index__crt_char_pattern = 0;
	}
	--times__before_next_char_pattern_update;

	foreach (var item in list_turrets)
		Echo(item.get_turret_info() + "\n");

	//遍历显示单元
	foreach (var item in list__display_units)
	{
		if (item.flag_graphic)
		{
			//图形化显示

			if (item.lcd.ContentType != ContentType.SCRIPT)
				continue;

			//switch (item.mode_display)
			//{
			//case DisplayUnit.DisplayMode.General:
			//	draw_illegal_lcd_custom_data_hint(item.lcd);
			//	break;
			//case DisplayUnit.DisplayMode.SingleTurret:
			//	draw_cannons_state(item.lcd, item.index_begin, item.index_begin);
			//	break;
			//case DisplayUnit.DisplayMode.MultipleTurret:
			//	draw_cannons_state(item.lcd, item.index_begin, item.index_end);
			//	break;
			//case DisplayUnit.DisplayMode.None:
			//	draw_illegal_lcd_custom_data_hint(item.lcd);
			//	break;
			//}
		}
		else
		{
			//非图形化显示

			if (item.lcd.ContentType != ContentType.TEXT_AND_IMAGE)
				continue;

			switch (item.mode_display)
			{
			case DisplayUnit.DisplayMode.General:
				item.lcd.WriteText(string_builder__default_info);
				break;
			case DisplayUnit.DisplayMode.SingleTurret:
				//item.lcd.WriteText(
				//	list__piston_cannons[item.index_begin].get_cannon_LCD_info());
				break;
			case DisplayUnit.DisplayMode.MultipleTurret:
				{
					//StringBuilder sb_temp = new StringBuilder();
					//for (var i = item.index_begin; i <= item.index_end; ++i)
					//	sb_temp.Append(list__piston_cannons[i].get_cannon_simple_info() + "\n");
					//item.lcd.WriteText(sb_temp);
				}
				break;
			case DisplayUnit.DisplayMode.None:
				item.lcd.WriteText("<warning> illegal custom data in this LCD\n<by> script AMPCCS");
				break;
			}
		}
	}

	//显示测试信息
	Echo("\n" + string_builder__test_info.ToString());

	return;
}

//初始化脚本
void init_script()
{
	init_script_config();//初始化脚本配置
	string str_error;
	//检查配置合法性
	if ((str_error = check_config()) != null) Echo(str_error);

	//构建网格图 传递program指针
	graph_grids = new GridsGraph(this);
	Echo(graph_grids.builder_str__init_info.ToString());

	int index_turret = 0;

	count__fixed_cameras = graph_grids.list_cameras.Count;
	//生成炮塔对象
	foreach (var item in graph_grids.list__turret_core_structures)
	{
		Turret turret = new Turret(this, item, index_turret);
		list_turrets.Add(turret); ++index_turret;
		count__fixed_cameras -= turret.struct_core.list_cameras.Count;
	}

	config_script.init_config();//初始化配置
	Echo(config_script.string_builder__error_info.ToString());

	if (graph_grids.list_controllers.Count != 0)
		vector__global_orientation = graph_grids.list_controllers[0].WorldMatrix.Forward;
	reset_global_orientation();

	foreach (var item in graph_grids.list_displayers)//遍历显示器
	{
		//拆分显示器的用户数据
		string[] array_str = split_string(item.CustomData);
		bool flag_illegal = false;
		int offset = 0;

		DisplayUnit unit = new DisplayUnit(item);

		if (array_str.Length == 0)
			unit.mode_display = DisplayUnit.DisplayMode.General;//用户数据为空
		else
		{
			if (array_str[array_str.Length - 1].Equals("graphic"))
			{ offset = 1; unit.flag_graphic = true; }
			//用户数据不为空
			switch (array_str[0])
			{
			case "graphic":
			case "general":
				unit.mode_display = DisplayUnit.DisplayMode.General;
				break;
			case "turret":
				{
					if (array_str.Length == 2 + offset)
					{
						int index = 0;
						if (!int.TryParse(array_str[1], out index))
						{
							flag_illegal = true;
							break;
						}
						//边界检查
						if (index < 0 || index > list_turrets.Count)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index;
						unit.mode_display = DisplayUnit.DisplayMode.SingleTurret;
					}
					else if (array_str.Length == 3 + offset)
					{
						int index_begin = 0, index_end = 0;
						if (!int.TryParse(array_str[1], out index_begin))
						{
							flag_illegal = true;
							break;
						}
						if (!int.TryParse(array_str[2], out index_end))
						{
							flag_illegal = true;
							break;
						}
						//边界检查
						if (index_begin < 0 || index_begin > list_turrets.Count)
						{
							flag_illegal = true;
							break;
						}
						if (index_end < 0 || index_end > list_turrets.Count)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index_begin;
						unit.index_end = index_end;
						unit.mode_display = DisplayUnit.DisplayMode.MultipleTurret;
					}
					else
						flag_illegal = true;
				}
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

	if (graph_grids.list_cameras.Count > 0)//计算最速扫描周期
	{
		increment__per_frame = graph_grids.list_cameras.Count * 2000.0 / 60;
		period__scan_gcm = (long)(distance__scan_under_global_control_mode / (increment__per_frame));
		period__scan_scm = (long)(distance__scan_under_single_control_mode / (increment__per_frame));
	}

	if(str_error == null && !config_script.flag__config_error)//检查是否出现错误
		Runtime.UpdateFrequency = UpdateFrequency.Update1;//设置执行频率 每1帧一次
	return;
}

void reset_global_orientation()
{
	if (list_turrets.Count != 0) foreach (var item in list_turrets) if (item.block__turret_indicator_main != null)
	{ vector__global_orientation = item.block__turret_indicator_main.WorldMatrix.Forward; break; }
}

//拆分字符串
string[] split_string(string str)
{
	string[] list_str = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
	return list_str;
}

//初始化脚本配置
void init_script_config()
{
	//脚本配置实例
	config_script = new CustomDataConfig(Me);

	config_set__script = new CustomDataConfigSet("SCRIPT CONFIGURATION");

	//添加配置项
	config_set__script.add_config_item(nameof(tag__script_core_group), () => tag__script_core_group, x => { tag__script_core_group = (string)x; });
	config_set__script.add_config_item(nameof(tag__elevation_stators), () => tag__elevation_stators, x => { tag__elevation_stators = (string)x; });
	config_set__script.add_config_item(nameof(tag__azimuth_stators), () => tag__azimuth_stators, x => { tag__azimuth_stators = (string)x; });

	config_set__script.add_config_item(nameof(flag__auto_check),() => flag__auto_check, x => { flag__auto_check = (bool)x; });
	config_set__script.add_config_item(nameof(flag__auto_fire_by_default),() => flag__auto_fire_by_default, x => { flag__auto_fire_by_default = (bool)x; });
	config_set__script.add_config_item(nameof(flag__scan_under_global_control_mode), () => flag__scan_under_global_control_mode, x => { flag__scan_under_global_control_mode = (bool)x; });
	config_set__script.add_config_item(nameof(flag__enable_global_control_mode_by_default), () => flag__enable_global_control_mode_by_default, x => { flag__enable_global_control_mode_by_default = (bool)x; });
	config_set__script.add_config_item(nameof(flag__lock_target_of_auto_turret),() => flag__lock_target_of_auto_turret, x => { flag__lock_target_of_auto_turret = (bool)x; });
	config_set__script.add_config_item(nameof(flag__lock_target_of_auto_turret_when_no_user), () => flag__lock_target_of_auto_turret_when_no_user, x => { flag__lock_target_of_auto_turret_when_no_user = (bool)x; });
	config_set__script.add_config_item(nameof(flag__reset_turret_angle_when_user_leave), () => flag__reset_turret_angle_when_user_leave, x => { flag__reset_turret_angle_when_user_leave = (bool)x; });
	config_set__script.add_config_item(nameof(flag__reset_global_orientation_when_user_enter), () => flag__reset_global_orientation_when_user_enter, x => { flag__reset_global_orientation_when_user_enter = (bool)x; });
	config_set__script.add_config_item(nameof(flag__asynchronously_control_by_default), () => flag__asynchronously_control_by_default, x => { flag__asynchronously_control_by_default = (bool)x; });
	config_set__script.add_config_item(nameof(flag__user_lose_turret_control_after_locking_by_default), () => flag__user_lose_turret_control_after_locking_by_default, x => { flag__user_lose_turret_control_after_locking_by_default = (bool)x; });
	config_set__script.add_config_item(nameof(flag__enable_azimuth_deflection_stabilizer), () => flag__enable_azimuth_deflection_stabilizer, x => { flag__enable_azimuth_deflection_stabilizer = (bool)x; });
	config_set__script.add_config_item(nameof(flag__enable_elevation_deflection_stabilizer), () => flag__enable_elevation_deflection_stabilizer, x => { flag__enable_elevation_deflection_stabilizer = (bool)x; });
	config_set__script.add_config_item(nameof(flag__enable_control_plane_crossing_prevention), () => flag__enable_control_plane_crossing_prevention, x => { flag__enable_control_plane_crossing_prevention = (bool)x; });
	config_set__script.add_config_item(nameof(flag__enable_n_raycast), () => flag__enable_n_raycast, x => { flag__enable_n_raycast = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_players), () => flag__ignore_players, x => { flag__ignore_players = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_rockets), () => flag__ignore_rockets, x => { flag__ignore_rockets = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_small_grids), () => flag__ignore_small_grids, x => { flag__ignore_small_grids = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_large_grids), () => flag__ignore_large_grids, x => { flag__ignore_large_grids = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_the_friendly), () => flag__ignore_the_friendly, x => { flag__ignore_the_friendly = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_the_neutral), () => flag__ignore_the_neutral, x => { flag__ignore_the_neutral = (bool)x; });
	config_set__script.add_config_item(nameof(flag__ignore_the_enemy), () => flag__ignore_the_enemy, x => { flag__ignore_the_enemy = (bool)x; });

	config_set__script.add_config_item(nameof(period__auto_check), () => period__auto_check, x => { period__auto_check = (int)x; });
	config_set__script.add_config_item(nameof(period__update_output), () => period__update_output, x => { period__update_output = (int)x; });

	config_set__script.add_config_item(nameof(degree__max_angle_tolerance_between_axis), () => degree__max_angle_tolerance_between_axis, x => { degree__max_angle_tolerance_between_axis = (double)x; });
	config_set__script.add_config_item(nameof(ratio__mouse_azimuth_sensitivity), () => ratio__mouse_azimuth_sensitivity, x => { ratio__mouse_azimuth_sensitivity = (double)x; });
	config_set__script.add_config_item(nameof(ratio__mouse_elevation_sensitivity), () => ratio__mouse_elevation_sensitivity, x => { ratio__mouse_elevation_sensitivity = (double)x; });
	config_set__script.add_config_item(nameof(distance__scan_under_global_control_mode), () => distance__scan_under_global_control_mode, x => { distance__scan_under_global_control_mode = (double)x; });
	config_set__script.add_config_item(nameof(distance__scan_under_single_control_mode), () => distance__scan_under_single_control_mode, x => { distance__scan_under_single_control_mode = (double)x; });
	config_set__script.add_config_item(nameof(vector_coeff), () => vector_coeff, x => { vector_coeff = (PIDCore.CoeffVector)x; });

	config_script.add_config_set(config_set__script);

	//初始化配置
	config_script.parse_custom_data(); return;

}

//检查脚本配置
string check_config()
{
	string info = "";
	return null;
}

#endregion

#region 类型定义

//枚举 用户操控模式
enum ControlMode
{
	GlobalControlMode,//全局操控模式
	SingleControlMode,//单一操控模式
	AutomaticControlMode,//全自动控制模式
	None,//不操控
}

//类 显示单元(当成结构体用)
class DisplayUnit
{
	//枚举 显示模式
	public enum DisplayMode
	{
		General,//一般信息显示
		SingleTurret,//单个炮塔信息显示
		MultipleTurret,//多个炮塔信息显示
		None,//不显示内容
	}

	//LCD显示器
	public IMyTextPanel lcd;

	public DisplayMode mode_display = DisplayMode.General;

	//索引 开始 (列表容器中的索引)
	public int index_begin = -1;
	//索引 末尾 (列表容器中的索引)
	public int index_end = -1;
	//标记 是否图形化显示
	public bool flag_graphic = false;

	//构造函数
	public DisplayUnit(IMyTextPanel _lcd, int _index_begin = -1, int _index_end = -1, bool _flag_graphic = false)
	{
		this.lcd = _lcd;
		this.flag_graphic = _flag_graphic;
		this.index_begin = _index_begin;
		this.index_end = _index_end;
		return;
	}
}

/**************************************************
* 类 SubTurret
* 一个 SubTurret 类实例对应一个上的一个子活动部件
**************************************************/
public class SubTurret
{
	public List<IMyMotorStator> list__elevation_stators_forward { get; private set; } = new List<IMyMotorStator>();
	public List<IMyMotorStator> list__elevation_stators_inverted { get; private set; } = new List<IMyMotorStator>();
	public List<IMyPistonBase> list__elevation_pistons_forward { get; private set; } = new List<IMyPistonBase>();
	public List<IMyPistonBase> list__elevation_pistons_inverted { get; private set; } = new List<IMyPistonBase>();
	//映射 定子的默认角度(回正角度)
	public Dictionary<IMyMotorStator, float> dict__stators_default_angle { get; private set; } = new Dictionary<IMyMotorStator, float>();

	public double error_elevation { get; set; } = 0;//误差 垂直误差
	PIDCore ins_pid_elevation;//PID实例 垂直
	Program program; Turret turret;
	
	public bool flag_valid { get; set; } = true;//标记 是否可用
	public IMyTerminalBlock block__turret_indicator { get; private set; } = null;//方块 方向指示器
	public IMyTerminalBlock block__elevation_normal_indicator { get; private set; } = null;//方块 垂直法向量指示器
	public bool flag__elevation_normal_indicator_inverted { get; private set; } = false;//标记 垂直法向量指示器倒置
	public SubTurretCoreStructure struct_core { get; private set; } = null;//核心结构
	StringBuilder string_builder__init_info = new StringBuilder();//字符串构建器 对象初始化时的信息

	public SubTurret(Program _program, SubTurretCoreStructure _struct_core, Turret _turret, Vector3D orientation__azimuth_main, PIDCore.CoeffVector vec_coeff)
	{
		program = _program;
		struct_core = _struct_core;
		turret = _turret;		

		ins_pid_elevation = new PIDCore(program, vec_coeff);

		if (struct_core.list_cameras.Count != 0 && block__turret_indicator == null)
			block__turret_indicator = struct_core.list_cameras.First();
		if (struct_core.list_weapons.Count != 0 && block__turret_indicator == null)
			block__turret_indicator = struct_core.list_weapons.First();
		if (struct_core.list_controllers.Count != 0 && block__turret_indicator == null)
			block__turret_indicator = struct_core.list_controllers.First();
		if (struct_core.list__other_blocks.Count != 0 && block__turret_indicator == null)
			block__turret_indicator = struct_core.list__other_blocks.First();

		if (block__turret_indicator == null)//找不到指示器
		{
			string_builder__init_info.Append($"<error> no indicator block found\n");
			flag_valid = false;
			return;//退出构造函数
		}

		//方向 垂直主要方向向量 垂直主要方向是水平主要方向和摄像头F向量的叉积方向
		Vector3D orientation__elevation_main = Vector3D.Zero;

		if (struct_core.list__elevation_stators.Count == 0)
		{
			string_builder__init_info.Append($"<error> no elevation(elevation) motors found\n");
			flag_valid = false;
		}
		else
		{
			if (block__turret_indicator != null)
				orientation__elevation_main = Vector3D.Cross(orientation__azimuth_main, block__turret_indicator.WorldMatrix.Forward);
			if (orientation__elevation_main.IsZero())//检查是否为0向量(当两个向量共线的时候, 叉积会是0)
			{
				//若共线, 则随机抽取一个垂直转子的U向量作为正向
				orientation__elevation_main = struct_core.list__elevation_stators.First().WorldMatrix.Up;
				string_builder__init_info.Append($"<warning> camera up vector and normal vector of elevation rotation plane are collinear\n");
			}
		}

		foreach (var item in struct_core.list_pistons)
			if (item.CustomData.Length == 0) list__elevation_pistons_forward.Add(item);
			else list__elevation_pistons_inverted.Add(item);

		foreach (var item in struct_core.list__elevation_stators)
		{
			bool flag_forward = true;//正反向标记
			double angle = (180 / Math.PI) * Math.Acos//计算与参照向量的夹角(转为角度制)
				(Vector3D.Dot(item.WorldMatrix.Up, orientation__elevation_main) /
				(item.WorldMatrix.Up.Length() * orientation__elevation_main.Length()));
			if (angle > 90.0)
			{
				flag_forward = false;
				angle = 180 - angle;
			}
			if (angle > program.degree__max_angle_tolerance_between_axis)
				continue;
			if (flag_forward)
				list__elevation_stators_forward.Add(item);
			else
				list__elevation_stators_inverted.Add(item);
		}
		//连接序列
		struct_core.list__elevation_stators = list__elevation_stators_forward.Concat(list__elevation_stators_inverted).ToList();
		foreach (var item in struct_core.list__elevation_stators)
		{
			float angle = 0;
			if (float.TryParse(item.CustomData, out angle))
			{
				Turret.normalize_angle_deg(ref angle);
				dict__stators_default_angle[item] = angle * (float)Math.PI / 180f;
			}
		}

		if (!set_normal_indicators())
		{
			string_builder__init_info.Append($"<error> cannot set normal indicators , consider if the motor(rotor) are broken");
			flag_valid = false;
		}

		int num = struct_core.list__elevation_stators.Count - list__elevation_stators_forward.Count - list__elevation_stators_inverted.Count;
		if (num > 0) string_builder__init_info.Append($"<warning> {num} elevation(elevation) motors are ignored for axis angle reason\n");
	}

	public void update()
	{
		if (turret.flag__time_to_auto_check)
		{
			//检查完整性
			if (!check_integrality())
			{
				flag_valid = false;
				return;
			}
			else if (!flag_valid)
				flag_valid = true;

			if (block__turret_indicator == null || !block__turret_indicator.IsWorking)
				set_indicator_block();
			if (block__turret_indicator == null || !block__turret_indicator.IsWorking)
			{
				flag_valid = false;
				return;
			}
		}
		if (!flag_valid) return;
	}

	public bool set_indicator_block()
	{
		if (struct_core.list_cameras.Count != 0 && (block__turret_indicator == null || !block__turret_indicator.IsFunctional))
			block__turret_indicator = struct_core.list_cameras.First();
		if (struct_core.list_weapons.Count != 0 && (block__turret_indicator == null || !block__turret_indicator.IsFunctional))
			block__turret_indicator = struct_core.list_weapons.First();
		if (struct_core.list_controllers.Count != 0 && (block__turret_indicator == null || !block__turret_indicator.IsFunctional))
			block__turret_indicator = struct_core.list_controllers.First();
		if (struct_core.list__other_blocks.Count != 0 && (block__turret_indicator == null || !block__turret_indicator.IsFunctional))
			block__turret_indicator = struct_core.list__other_blocks.First();
		if (block__turret_indicator == null || !block__turret_indicator.IsFunctional)
			return false;
		return true;
	}

	public void pid_control()
	{
		double output_v = -ins_pid_elevation.cal_output(error_elevation);
		foreach (var item in list__elevation_stators_forward)
			item.TargetVelocityRPM = (float)output_v;
		foreach (var item in list__elevation_stators_inverted)
			item.TargetVelocityRPM = -(float)output_v;
		foreach (var item in list__elevation_pistons_forward)
			item.Velocity = (float)output_v;
		foreach (var item in list__elevation_pistons_inverted)
			item.Velocity = -(float)output_v;
	}

	//设置法向量指示器
	private bool set_normal_indicators()
	{
		if (block__elevation_normal_indicator == null || !block__elevation_normal_indicator.IsFunctional)
			foreach (var item in list__elevation_stators_forward)
				if (item.IsFunctional)
				{
					block__elevation_normal_indicator = item;
					flag__elevation_normal_indicator_inverted = false;
					break;
				}
		if (block__elevation_normal_indicator == null || !block__elevation_normal_indicator.IsFunctional)
			foreach (var item in list__elevation_stators_inverted)
				if (item.IsFunctional)
				{
					block__elevation_normal_indicator = item;
					flag__elevation_normal_indicator_inverted = true;
					break;
				}
		if (block__elevation_normal_indicator == null || !block__elevation_normal_indicator.IsFunctional)
			return false;
		else
			return true;
	}

	public bool check_integrality()
	{
		//结果
		bool res = false;
		//检查水平电机定子完整性
		foreach (var item in struct_core.list__elevation_stators)
			if (item.IsFunctional)
			{
				res = true; break;
			}
		return res;
	}

	public string get_info()
	{
		return "<moving_parts> " + (flag_valid ? "working" : "invalid")
			+ "\n<error_elevation> " + (error_elevation * 180 / Math.PI).ToString("f5")
			+ "\n<count__forward_elevation_stators> " + list__elevation_stators_forward.Count
			+ "\n<count__inverted_elevation_stators> " + list__elevation_stators_inverted.Count
			+ "\n<count__forward_evelation_pistons> " + list__elevation_pistons_forward.Count
			+ "\n<count__inverted_evelation_pistons> " + list__elevation_pistons_inverted.Count
			+ "\n<count_cameras> " + struct_core.list_cameras.Count
			+ "\n<count_weapons> " + struct_core.list_weapons.Count
			+ "\n<count_controllers> " + struct_core.list_controllers.Count
			+ "\n<count_other_blocks> " + struct_core.list__other_blocks.Count
			+ "\n<flag__valid_indicator> " + (block__turret_indicator != null)
			+ "\n<count_grids> " + struct_core.set_grids.Count
			+ "\n <init_info> \n" + string_builder__init_info.ToString() + "\n\n";
	}
}

/**************************************************
* 类 Turret
* 一个 Turret 类实例对应一个炮塔
**************************************************/
public class Turret
{
	//枚举 炮塔内部驱动模式
	private enum TurretDrivingMode
	{
		GlobalCoordinateDrivingMode,//全局坐标驱动模式
		GlobalOrientationDrivingMode,//全局方向驱动模式
		LocalAngleDrivingMode,//本地角度驱动模式
		None,//不控制
	}

	//枚举 炮塔状态
	public enum TurretStatus
	{
		Idle,//闲置中(没有目标)
		Locating,//定位中(炮塔正在旋转)
		Ready,//就绪(成功抵达目标位置)
		BrokenDown,//故障(关键元件无法工作)
		Invalid,//不可用(炮塔初始化时缺少关键元件)
	}

	//枚举 炮塔指令(炮塔接下来需要执行的指令)
	public enum TurretCommand
	{
		Locate,//定位
		Reset,//复位
		None,//无指令
	}

	//向量 目标向量
	public Vector3D vector_target_direction { get; private set; } = new Vector3D();
	public Vector3D vector_target_coordinate { get; private set; } = new Vector3D();

	List<IMyMotorStator> list__azimuth_stators = new List<IMyMotorStator>();//列表 该炮塔上的全部水平电机定子
	List<IMyMotorStator> list__azimuth_stators_forward = new List<IMyMotorStator>();//列表 该炮塔上的全部正向水平电机定子
	List<IMyMotorStator> list__azimuth_stators_inverted = new List<IMyMotorStator>();//列表 该炮塔上的全部反向水平电机定子
	Dictionary<IMyMotorStator, float> dict__stators_default_angle = new Dictionary<IMyMotorStator, float>();//映射 定子的默认角度(回正角度)
	Dictionary<IMyMotorStator, float> dict__stators_dead_zone_bisector = new Dictionary<IMyMotorStator, float>();//映射 定子的盲区边界平分线
	List<SubTurret> list_subturrets = new List<SubTurret>();//列表 子炮塔
	public IMyTerminalBlock block__turret_indicator_main { get; private set; } = null;//方块 方向指示器
	public IMyTerminalBlock block__azimuth_normal_indicator { get; private set; } = null;//方块 水平法向量指示器
	bool flag__azimuth_normal_indicator_inverted = false;//标记 水平法向量指示器倒置
	double error_azimuth = 0.0;//误差 水平误差
	PIDCore ins_pid_azimuth;//PID实例 水平
	public PIDCore.CoeffVector vector_coeff { get; private set; } = new PIDCore.CoeffVector();//向量 系数向量
	public int index_turret { get; private set; } = -1;//索引 炮塔的编号

	bool flag__enable_anticipation = true;//标记 是否 进行预判
	bool flag__inherit_vehicle_speed = true;//标记 是否 继承载具速度
	bool flag__affected_by_gravity = false;//标记 是否 受重力影响
	bool flag_consider_target_acceleration = true;//标记 是否 考虑目标加速度
	double velocity_initial__projectile = 400;//速度 抛射物初速
	double velocity_max__projectile = 400;//速度 抛射物最大速度
	double acceleration_projectile = 0;//加速度 抛射物加速度
	double angle__turret_azimuth_reset = 0;//旋转角 炮塔复位水平旋转角
	double angle__turret_elevation_reset = 0;//旋转角 炮塔复位垂直旋转角

	public TurretCoreStructure struct_core { get; private set; } = null;//核心结构

	CustomDataConfigSet config_turret = null;
	Program program;//编程块的this指针
	int times__before_next_check = 0;//次数 距离下一次 检查

	public MyDetectedEntityInfo entity_info__target { get; private set; }//实体信息 目标实体
	TurretDrivingMode mode_driving = TurretDrivingMode.GlobalOrientationDrivingMode;//模式 驱动模式
	TurretStatus status_turret = TurretStatus.Idle;//状态 炮塔状态
	TurretCommand command_turret = TurretCommand.Locate;//指令 炮塔指令
	public bool flag__time_to_auto_check { get; private set; } = false;//标记 是否 应该进行自动检查
	StringBuilder string_builder__init_info = new StringBuilder();//字符串构建器 对象初始化时的信息

	public Turret(Program _program, TurretCoreStructure _struct_core, int _index_turret)//构造函数
	{
		program = _program; struct_core = _struct_core; index_turret = _index_turret;
		vector_coeff.p = program.vector_coeff.p;
		vector_coeff.i = program.vector_coeff.i;
		vector_coeff.d = program.vector_coeff.d;
		ins_pid_azimuth = new PIDCore(program, vector_coeff);


		if (struct_core.list_cameras.Count == 0)//检查
			string_builder__init_info.Append($"<warning> no cameras found in turret #{index_turret} , ignored\n");
		if (struct_core.list_sensors.Count == 0)//检查
			string_builder__init_info.Append($"<info> no sensors found in turret #{index_turret} , ignored\n");
		if (struct_core.list_weapons.Count == 0)//检查
			string_builder__init_info.Append($"<info> no weapon blocks found in turret #{index_turret} , ignored\n");
		if (struct_core.list_gyros.Count == 0)//检查
			string_builder__init_info.Append($"<info> no gyroscopes found in turret #{index_turret} , ignored\n");
		if (struct_core.list_timers.Count == 0)//检查
			string_builder__init_info.Append($"<info> no timers found in turret #{index_turret} , ignored\n");

		//方向 水平主要方向向量 随机选取一个逻辑正向安装类型的水平电机, 若没有正向的就选择反向并取反
		Vector3D orientation__azimuth_main = Vector3D.Zero;

		if(struct_core.dict__azimuth_stators_logical.Count == 0)
		{
			string_builder__init_info.Append($"<error> no azimuth(azimuth) motors found in turret #{index_turret}\n");
			status_turret = TurretStatus.Invalid;
		}
		else
		{
			foreach (var item in struct_core.dict__azimuth_stators_logical)//找到一个正向安装的
				if (item.Value == MotorMountingType.Forward)
				{
					orientation__azimuth_main = Vector3D.Normalize(item.Key.WorldMatrix.Up);
					break;
				}
			if (orientation__azimuth_main.Equals(Vector3D.Zero))//没找到正向安装的, 则选取第一个, 取反其U向量
				orientation__azimuth_main = -Vector3D.Normalize(struct_core.dict__azimuth_stators_logical.First().Key.WorldMatrix.Up);
		}

		//检查各个定子的方向和共线性, 若超过容忍限度视为不可用, 并忽略
		foreach(var item in struct_core.dict__azimuth_stators_logical)
		{
			bool flag_forward = true;//正反向标记
			double angle = (180 / Math.PI) * Math.Acos//计算与参照向量的夹角(转为角度制)
				(Vector3D.Dot(item.Key.WorldMatrix.Up, orientation__azimuth_main) /
				(item.Key.WorldMatrix.Up.Length() * orientation__azimuth_main.Length()));
			if (angle > 90.0)
			{
				flag_forward = false;
				angle = 180 - angle;
			}
			if (angle > program.degree__max_angle_tolerance_between_axis)
				continue;
			if (flag_forward)
				list__azimuth_stators_forward.Add(item.Key);
			else
				list__azimuth_stators_inverted.Add(item.Key);
		}

		//连接序列
		list__azimuth_stators = list__azimuth_stators_forward.Concat(list__azimuth_stators_inverted).ToList();
		foreach (var item in list__azimuth_stators)//读取默认角度; 设置盲区平分线
		{
			float angle = 0;
			if (float.TryParse(item.CustomData, out angle))
			{ normalize_angle_deg(ref angle); dict__stators_default_angle[item] = angle * (float)Math.PI / 180f; }
			var tmp = item.UpperLimitRad - item.LowerLimitRad;
			if (item.UpperLimitRad != float.MaxValue && item.LowerLimitRad != float.MinValue && tmp > Math.PI && tmp < 2 * Math.PI)
				dict__stators_dead_zone_bisector[item] = (float)((item.UpperLimitRad + item.LowerLimitRad) / 2 + Math.PI);
			//throw new Exception("" + dict__stators_dead_zone_bisector[item] * 57.3);
		}

		//构建炮塔配置
		config_turret = new CustomDataConfigSet($"TURRET #{index_turret} CONFIGURATION");
		//config_turret.add_config_item(nameof(index_turret), () => index_turret, x => { index_turret = (int)x; });
		config_turret.add_config_item(nameof(flag__inherit_vehicle_speed), () => flag__inherit_vehicle_speed, x => { flag__inherit_vehicle_speed = (bool)x; });
		config_turret.add_config_item(nameof(velocity_initial__projectile), () => velocity_initial__projectile, x => { velocity_initial__projectile = (double)x; });
		config_turret.add_config_item(nameof(velocity_max__projectile), () => velocity_max__projectile, x => { velocity_max__projectile = (double)x; });
		config_turret.add_config_item(nameof(acceleration_projectile), () => acceleration_projectile, x => { acceleration_projectile = (double)x; });
		config_turret.add_config_item(nameof(angle__turret_azimuth_reset), () => angle__turret_azimuth_reset, x => { angle__turret_azimuth_reset = (double)x; });
		config_turret.add_config_item(nameof(angle__turret_elevation_reset), () => angle__turret_elevation_reset, x => { angle__turret_elevation_reset = (double)x; });
		config_turret.add_config_item(nameof(vector_coeff), () => vector_coeff, x => { vector_coeff = (PIDCore.CoeffVector)x; });
		program.config_script.add_config_set(config_turret);

		foreach (var item in struct_core.list__subturret)//构建子炮塔对象
			list_subturrets.Add(new SubTurret(program, item, this, orientation__azimuth_main, vector_coeff));
		
		if (list_subturrets.Count == 0)
		{
			string_builder__init_info.Append($"<error> no moving parts (elevation motors) found in turret #{index_turret}");
			status_turret = TurretStatus.Invalid;
		}
		else
		{
			set_turret_main_indicator_block();
			vector_target_direction = list_subturrets[0].block__turret_indicator.WorldMatrix.Forward;
		}
			
		if (!set_normal_indicators())
		{
			string_builder__init_info.Append($"<error> cannot set normal indicators in #{index_turret}, consider if the motor(rotor) are broken");
			status_turret = TurretStatus.Invalid;
		}

		int num = struct_core.dict__azimuth_stators_logical.Count - list__azimuth_stators_forward.Count - list__azimuth_stators_inverted.Count;
		if (num > 0)
			string_builder__init_info.Append($"<warning> {num} azimuth(azimuth) motors are ignored for axis angle reason\n");
	}

	public void update()//更新炮塔
	{
		if (status_turret == TurretStatus.Invalid) return;

		//自动检查
		if (program.flag__auto_check)
		{
			if (times__before_next_check == 0)
			{
				flag__time_to_auto_check = true;
				times__before_next_check = program.period__auto_check;
			}
			--times__before_next_check;
		}

		if (flag__time_to_auto_check)
		{
			bool flag = check_integrality();
			if (flag)
			{
				flag = false;
				foreach (var item in list_subturrets)
				{
					if (!item.check_integrality())
					{
						item.flag_valid = false;
						return;
					}
					else if (!item.flag_valid)
						item.flag_valid = true;

					if (item.block__turret_indicator == null || !item.block__turret_indicator.IsFunctional)
						item.set_indicator_block();
					if (item.block__turret_indicator == null || !item.block__turret_indicator.IsFunctional)
						item.flag_valid = false;

					if (item.flag_valid)
					{
						flag = true;
						break;
					}
				}
			}

			//检查完整性
			if (!flag)
				status_turret = TurretStatus.BrokenDown;//炮塔不完整
			else if (status_turret == TurretStatus.BrokenDown)
				status_turret = TurretStatus.Idle;//之前不完整现在完整, 更新状态

			if (block__turret_indicator_main == null && !set_turret_main_indicator_block())
			{
				status_turret = TurretStatus.BrokenDown;
				return;
			}
		}

		if (status_turret == TurretStatus.BrokenDown) return;
			
		if (block__azimuth_normal_indicator == null || !block__azimuth_normal_indicator.IsFunctional)
			if (!set_normal_indicators())
			{
				status_turret = TurretStatus.BrokenDown;
				return;
			}

		if (mode_driving != TurretDrivingMode.LocalAngleDrivingMode)
		{
			if (block__turret_indicator_main != null)
			{
				if (mode_driving == TurretDrivingMode.GlobalCoordinateDrivingMode)
					vector_target_direction = vector_target_coordinate - block__turret_indicator_main.CubeGrid.GridIntegerToWorld(block__turret_indicator_main.Position);
				//计算误差
				error_azimuth = cal_error_angle(flag__azimuth_normal_indicator_inverted ? block__azimuth_normal_indicator.WorldMatrix.Down
					: block__azimuth_normal_indicator.WorldMatrix.Up, block__turret_indicator_main.WorldMatrix.Forward, vector_target_direction);
				var quaternion = Quaternion.CreateFromAxisAngle(block__azimuth_normal_indicator.WorldMatrix.Up, (float)(flag__azimuth_normal_indicator_inverted ? -error_azimuth : error_azimuth));
				foreach (var item in list_subturrets)//子炮塔
					if (item.flag_valid)
					{
						var matrix_world = item.block__elevation_normal_indicator.WorldMatrix;
						MatrixD.Transform(ref matrix_world, ref quaternion, out matrix_world);
						item.error_elevation = cal_error_angle(item.flag__elevation_normal_indicator_inverted ? matrix_world.Down : matrix_world.Up,
						Vector3D.Transform(item.block__turret_indicator.WorldMatrix.Forward, quaternion), vector_target_direction);
					}
			}
		}
		else//本地角度模式(自动回正)
		{
			error_azimuth = 0;
			if (dict__stators_default_angle.Count != 0)
			{
				foreach (var item in list__azimuth_stators_forward)
					if (dict__stators_default_angle.ContainsKey(item)) error_azimuth += item.Angle - dict__stators_default_angle[item];
				foreach (var item in list__azimuth_stators_inverted)
					if (dict__stators_default_angle.ContainsKey(item)) error_azimuth -= item.Angle - dict__stators_default_angle[item];
				error_azimuth = normalize_angle_rad((float)error_azimuth) / dict__stators_default_angle.Count;//取平均值
			}
			foreach (var item in list_subturrets)//子炮塔
				if (item.flag_valid)
				{
					item.error_elevation = 0;
					if (item.dict__stators_default_angle.Count != 0)
					{
						foreach (var item1 in item.list__elevation_stators_forward)
							if (item.dict__stators_default_angle.ContainsKey(item1)) item.error_elevation += item1.Angle - item.dict__stators_default_angle[item1];
						foreach (var item1 in item.list__elevation_stators_inverted)
							if (item.dict__stators_default_angle.ContainsKey(item1)) item.error_elevation -= item1.Angle - item.dict__stators_default_angle[item1];
						item.error_elevation = normalize_angle_rad((float)item.error_elevation) / item.dict__stators_default_angle.Count;//取平均值
					}
				}
		}

		//规避盲区(仅水平方向)
		foreach (var item in dict__stators_dead_zone_bisector)
		{
			var tmp = normalize_angle_rad(item.Value - item.Key.Angle);
			if (Math.Sign(tmp) != Math.Sign(error_azimuth) && Math.Abs(tmp) < Math.Abs(error_azimuth))
			{ error_azimuth += error_azimuth > 0 ? -2 * Math.PI : 2 * Math.PI; break; }
		}

		pid_control();
		foreach (var item in list_subturrets)
			if (item.flag_valid)
				item.pid_control();
		return;
	}
	//设置目标实体
	public void set_target_entity(MyDetectedEntityInfo _entity_info__target)
	{
		//设为全局坐标驱动模式
		mode_driving = TurretDrivingMode.GlobalCoordinateDrivingMode;
		entity_info__target = _entity_info__target;
		vector_target_coordinate = (Vector3D)entity_info__target.HitPosition;
	}
	//设置目标朝向
	public void set_target_orientation(Vector3D vector__global_orientation)
	{
		this.vector_target_direction = vector__global_orientation;
		//设为全局方向驱动模式
		this.mode_driving = TurretDrivingMode.GlobalOrientationDrivingMode;
	}
	//回正
	public void reset() => this.mode_driving = TurretDrivingMode.LocalAngleDrivingMode;
	//检查是否正被控制
	public bool check_if_under_control()
	{
		bool res = false;
		foreach (var item in struct_core.list_cameras)
			if (item.IsActive)
			{
				res = true;
				break;
			}
		return res;
	}

	private void pid_control()
	{
		double output_h = -ins_pid_azimuth.cal_output(error_azimuth);//误差为正需要逆时针则输出为负值
		foreach (var item in list__azimuth_stators_forward)
			item.TargetVelocityRPM = (float)output_h;
		foreach (var item in list__azimuth_stators_inverted)
			item.TargetVelocityRPM = -(float)output_h;
		return;
	}
	//设置法向量指示器
	private bool set_normal_indicators()
	{
		if (block__azimuth_normal_indicator == null || !block__azimuth_normal_indicator.IsFunctional)
			foreach (var item in list__azimuth_stators_forward)
				if (item.IsFunctional)
				{
					block__azimuth_normal_indicator = item;
					flag__azimuth_normal_indicator_inverted = false;
					break;
				}
		if (block__azimuth_normal_indicator == null || !block__azimuth_normal_indicator.IsFunctional)
			foreach (var item in list__azimuth_stators_inverted)
				if (item.IsFunctional)
				{
					block__azimuth_normal_indicator = item;
					flag__azimuth_normal_indicator_inverted = true;
					break;
				}
		if (block__azimuth_normal_indicator == null || !block__azimuth_normal_indicator.IsFunctional)
			return false;
		return true;
	}

	private bool set_turret_main_indicator_block()
	{
		if (block__turret_indicator_main == null)
			foreach (var item in list_subturrets)
			{
				if (item.block__turret_indicator != null)
					block__turret_indicator_main = item.block__turret_indicator;
				break;
			}
		if (block__turret_indicator_main == null)
			return false;
		return true;
	}
	//检查完整性
	private bool check_integrality()
	{
		bool res = false;//结果
		foreach (var item in list__azimuth_stators)//检查水平电机定子完整性
			if (item.IsFunctional)
			{
				res = true;
				break;
			}
		return res;
	}
	//获取炮塔信息
	public string get_turret_info()
	{
		StringBuilder b = new StringBuilder();
		b.Append("<turret> ------------------------------ No." + this.index_turret
			+ "\n<status> " + status_turret.ToString()
			+ "\n<vector_target> " + vector_target_direction.X.ToString("f2") + " , " + vector_target_direction.Y.ToString("f2") + " , " + vector_target_direction.Z.ToString("f2")
			+ "\n<error_azimuth> " + (error_azimuth * 180 / Math.PI).ToString("f3")
			+ "\n<count__forward_azimuth_stators> " + list__azimuth_stators_forward.Count
			+ "\n<count__inverted_azimuth_stators> " + list__azimuth_stators_inverted.Count
			+ "\n<count_cameras> " + struct_core.list_cameras.Count
			+ "\n<count_grids> " + struct_core.set_grids.Count
			+ "\n<count___moving_parts> " + list_subturrets.Count);
		b.Append("\n <init_info> \n" + string_builder__init_info.ToString() + "\n\n");
		foreach (var item in list_subturrets)
			b.Append(item.get_info());

		return b.ToString();
	}

	public void check()
	{
		string_builder__init_info.Append($"{{{program.Runtime.MaxInstructionCount - program.Runtime.CurrentInstructionCount}, {program.Runtime.MaxCallChainDepth - program.Runtime.CurrentCallChainDepth} }}");
	}

	public static float normalize_angle_rad(float angle)//规范化角度(弧度制)
	{ while (angle > Math.PI) angle -= (float)Math.PI * 2; while (angle < -Math.PI) angle += (float)Math.PI * 2; return angle; }

	public static void normalize_angle_deg(ref float angle)//规范化角度(角度制)
	{ while (angle > 180) angle -= 360; while (angle < -180) angle += 360; }

	//计算误差角
	//参数: 旋转面法向量; 指示器向量; 目标方向向量
	//返回值: 正数表示需逆时针旋转, 反之顺时针(弧度)
	public static double cal_error_angle(Vector3D vector_normal, Vector3D vector_indicator, Vector3D vector_direction)
	{
		Vector3D vector_crt = Vector3D.ProjectOnPlane(ref vector_indicator, ref vector_normal);
		Vector3D vector_target = Vector3D.ProjectOnPlane(ref vector_direction, ref vector_normal);
		Vector3D vector_ref = Vector3D.Cross(vector_target, vector_crt);//叉积
		double num = Vector3D.Dot(vector_crt, vector_target) / (vector_crt.Length() * vector_target.Length());
		if (num > 1.0) num = 1.0; if (num < -1.0) num = -1.0;
		double angle_error = Math.Acos(num);
		return Vector3D.Dot(vector_normal, vector_ref) > 0 ? -angle_error : angle_error;
	}

}

//枚举 电机安装类型
public enum MotorMountingType
{
	Forward,//正向安装
	Inverted,//倒置安装
	Invalid,//不可用的
	None,//未知
}

/**************************************************
* 类 SubTurretCoreStructure
* 子炮塔核心结构
**************************************************/
public class SubTurretCoreStructure
{
	//列表(多个) 各类元件
	public List<IMyShipController> list_controllers = new List<IMyShipController>();
	public List<IMyCameraBlock> list_cameras = new List<IMyCameraBlock>();
	public List<IMySensorBlock> list_sensors = new List<IMySensorBlock>();
	public List<IMyUserControllableGun> list_weapons = new List<IMyUserControllableGun>();
	public List<IMyGyro> list_gyros = new List<IMyGyro>();
	public List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();
	public List<IMyTerminalBlock> list__other_blocks = new List<IMyTerminalBlock>();
	//列表 高低机(转子, 活塞)
	public List<IMyMotorStator> list__elevation_stators = new List<IMyMotorStator>();
	
	//哈希集合 炮塔全部网格
	public HashSet<IMyCubeGrid> set_grids = new HashSet<IMyCubeGrid>();

	public bool flag_valid = true;
}

/**************************************************
* 类 TurretCoreStructure
* 炮塔核心结构, 将这个对象传递给类Turret的构造函数来构造一个炮塔
* 本类实例包含了炮塔的全部网格对象, 水平电机和垂直电机对象
**************************************************/
public class TurretCoreStructure
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

	public List<SubTurretCoreStructure> list__subturret = new List<SubTurretCoreStructure>();

	//哈希集合 炮塔全部网格
	public HashSet<IMyCubeGrid> set_grids = new HashSet<IMyCubeGrid>();

	public bool flag_valid = true;
}

/**************************************************
* 类 GridsGraph
* 网格图
* 非泛型通用类, 为本脚本设计, 无法移植
* 实例中包含本脚本所需的全部对象列表
**************************************************/
public class GridsGraph
{
	//类 网格节点
	public class Node
	{
		public int index;//索引 List容器中的索引
		public IMyCubeGrid grid;//网格
		public bool flag_turret;//标记 炮塔网格连通性
		public bool flag_subturret;//标记 子炮塔网格连通性
		//列表(多个) 该网格上的各类元件
		public List<IMyShipController> list_controllers = new List<IMyShipController>();
		public List<IMyCameraBlock> list_cameras = new List<IMyCameraBlock>();
		public List<IMySensorBlock> list_sensors = new List<IMySensorBlock>();
		public List<IMyUserControllableGun> list_weapons = new List<IMyUserControllableGun>();
		public List<IMyGyro> list_gyros = new List<IMyGyro>();
		public List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();
		public List<IMyTimerBlock> list_timers = new List<IMyTimerBlock>();
		public List<IMyTerminalBlock> list__other_blocks = new List<IMyTerminalBlock>();

		public Node(int _index, IMyCubeGrid _grid = null)
		{ index = _index; grid = _grid; }
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

	//编组 脚本核心方块
	IMyBlockGroup group__script_core = null;
	//列表 所有方块
	public List<IMyTerminalBlock> list_blocks { get; private set; } = new List<IMyTerminalBlock>();
	//列表 全部机械连接方块(排除悬架)
	public List<IMyMechanicalConnectionBlock> list_mcbs { get; private set; } = new List<IMyMechanicalConnectionBlock>();
	//列表 全部控制器
	public List<IMyShipController> list_controllers { get; private set; } = new List<IMyShipController>();
	//列表 全部电机定子
	public List<IMyMotorStator> list_stators { get; private set; } = new List<IMyMotorStator>();
	//列表 全部水平电机定子
	public List<IMyMotorStator> list__azimuth_stators { get; private set; } = new List<IMyMotorStator>();
	//列表 全部垂直电机定子
	public List<IMyMotorStator> list__elevation_stators { get; private set; } = new List<IMyMotorStator>();
	//列表 全部摄像头
	public List<IMyCameraBlock> list_cameras { get; private set; } = new List<IMyCameraBlock>();
	//列表 全部探测器
	public List<IMySensorBlock> list_sensors { get; private set; } = new List<IMySensorBlock>();
	//列表 全部武器
	public List<IMyUserControllableGun> list_weapons { get; private set; } = new List<IMyUserControllableGun>();
	//列表 全部自动炮塔
	public List<IMyLargeTurretBase> list__auto_turrets { get; private set; } = new List<IMyLargeTurretBase>();
	//列表 陀螺仪
	public List<IMyGyro> list_gyros { get; private set; } = new List<IMyGyro>();
	//列表 活塞
	public List<IMyPistonBase> list_pistons { get; private set; } = new List<IMyPistonBase>();
	//列表 定时器
	public List<IMyTimerBlock> list_timers { get; private set; } = new List<IMyTimerBlock>();
	//列表 显示器
	public List<IMyTextPanel> list_displayers { get; private set; } = new List<IMyTextPanel>();
	//列表 其它方块
	public List<IMyTerminalBlock> list__other_blocks { get; private set; } = null;

	//列表 图的节点
	public List<Node> list_nodes { get; private set; } = new List<Node>();
	//字典 根据网格快速检索节点索引
	Dictionary<IMyCubeGrid, int> dict__grids_index = new Dictionary<IMyCubeGrid, int>();
	//字典 根据电机查找它处在的边的索引
	Dictionary<IMyMotorStator, Indexes> dict__edge_index_of_motor = new Dictionary<IMyMotorStator, Indexes>();
	//列表 炮塔核心结构列表 执行构造函数之后会生成此列表
	public List<TurretCoreStructure> list__turret_core_structures { get; private set; } = new List<TurretCoreStructure>();
	public List<SubTurretCoreStructure> list__subturret_core_structures { get; private set; } = new List<SubTurretCoreStructure>();

	//字典 存储所有边, 考虑到用邻接矩阵来表示稀疏图, 时间空间浪费过大, 因此转而使用邻接表
	List<Dictionary<int, Edge>> list_adjacency = new List<Dictionary<int, Edge>>();

	Program program = null;
	//字符串构建器 初始化信息
	public StringBuilder builder_str__init_info { get; private set; } = new StringBuilder();

	//构造函数
	public GridsGraph(Program _program)
	{
		//成员赋值
		this.program = _program;
		//获取脚本核心编组
		group__script_core = program.GridTerminalSystem.GetBlockGroupWithName(program.tag__script_core_group);
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
		foreach (var item in list_cameras)
		{ item.EnableRaycast = true; if (program.flag__enable_n_raycast) item.Raycast(Double.NaN); }
		
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
		program.GridTerminalSystem.GetBlocksOfType(list_temp);//获取全部机械连接方块(电机, 铰链, 活塞, 悬架)
		foreach (var item in list_temp) if (!(item is IMyMotorSuspension)) list_mcbs.Add(item);

		int index = 0;
		//获取全部网格 构建节点列表和节点索引
		foreach (var item in list_mcbs)
		{
			if (!dict__grids_index.ContainsKey(item.CubeGrid))
			{
				list_nodes.Add(new Node(index, item.CubeGrid));
				dict__grids_index[item.CubeGrid] = index++;
				builder_str__init_info.Append($"<info> node {index - 1} {item.CubeGrid.CustomName}\n");
			}
			if (item.TopGrid != null && !dict__grids_index.ContainsKey(item.TopGrid))
			{
				list_nodes.Add(new Node(index, item.TopGrid));
				dict__grids_index[item.TopGrid] = index++;
				builder_str__init_info.Append($"<info> node {index - 1} {item.TopGrid.CustomName}\n");
			}
		}

		//区分水平电机定子和垂直电机定子, 添加到对应列表中, 未标注的定子将被忽略
		foreach (var item in list_stators)
			if (item.CustomName.Contains(program.tag__azimuth_stators))
				list__azimuth_stators.Add(item);//添加到列表
			else if (item.CustomName.Contains(program.tag__elevation_stators))
				list__elevation_stators.Add(item);//添加到列表

		//构建邻接表 不再使用邻接矩阵, 邻接表无需初始化
		for (int i = 0; i < list_nodes.Count; ++i)
			list_adjacency.Add(new Dictionary<int, Edge>());//创建每个节点的临边字典

		//向邻接表中添加边
		foreach (var item in list_mcbs)
		{
			if (item.CustomData.StartsWith("connection blocked") || item.TopGrid == null)
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
			if (list_nodes[index].flag_subturret)
				continue;//被搜索过了
			bfs_subturret(index);
		}
		foreach (var item in list_weapons)//BFS搜索
		{
			index = dict__grids_index[item.CubeGrid];
			if (list_nodes[index].flag_subturret)
				continue;//被搜索过了
			bfs_subturret(index);
		}
		foreach (var item in list_controllers)//BFS搜索
		{
			index = dict__grids_index[item.CubeGrid];
			if (list_nodes[index].flag_subturret)
				continue;//被搜索过了
			bfs_subturret(index);
		}
		foreach (var item in list__other_blocks)//BFS搜索
		{
			index = dict__grids_index[item.CubeGrid];
			if (list_nodes[index].flag_subturret)
				continue;//被搜索过了
			bfs_subturret(index);
		}
		
		//搜索液压杆
		foreach (var item in list__subturret_core_structures)
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

	private void bfs_subturret(int index_start)
	{
		Queue queue = new Queue();//队列 索引队列
		queue.Enqueue(index_start);//起点入队列
		list_nodes[index_start].flag_subturret = true;
		SubTurretCoreStructure struct__subturret_core = new SubTurretCoreStructure();//炮塔核心结构

		builder_str__init_info.Append($"<info> bfs_subturret(): start {index_start} {list_nodes[index_start].grid.CustomName}\n");
		int index_next = 0;//下一个索引
		while (queue.Count != 0)//主循环
		{
			int index_crt = (int)queue.Dequeue();//索引 当前索引
			Node node_crt = list_nodes[index_crt];//当前节点
			struct__subturret_core.set_grids.Add(node_crt.grid);
			builder_str__init_info.Append($"<info> -> {index_crt} {list_nodes[index_crt].grid.CustomName}\n");

			struct__subturret_core.list_controllers.AddRange(node_crt.list_controllers);
			struct__subturret_core.list_cameras.AddRange(node_crt.list_cameras);
			struct__subturret_core.list_sensors.AddRange(node_crt.list_sensors);
			struct__subturret_core.list_weapons.AddRange(node_crt.list_weapons);
			struct__subturret_core.list_gyros.AddRange(node_crt.list_gyros);
			//struct__subturret_core.list_pistons.AddRange(node_crt.list_pistons);
			struct__subturret_core.list__other_blocks.AddRange(node_crt.list__other_blocks);

			if (node_crt.grid.CustomName.Contains(program.tag__elevation_pistons_gird))
				search_pistons(index_crt, struct__subturret_core);//搜索活塞

			//遍历出边 找到没有阻塞性, 并且抵达的网格没有被标记过的的出边
			for (index_next = 0; index_next < list_nodes.Count; ++index_next)
			{
				if (!list_adjacency[index_crt].ContainsKey(index_next)) continue;//不是出边就跳过
				Node node_next = list_nodes[index_next];//下一个节点
				Edge edge = list_adjacency[index_crt][index_next];//边
				if (edge.flag__has_azimuth_motor) break;//发现水平电机退出
				if (edge.flag__has_elevation_motor)
				{
					//这条边上存在高低机, 把它们添加到核心结构对象中
					struct__subturret_core.list__elevation_stators.AddRange(edge.list_stators);
					continue;//跳过
				}
				//检查是否已经进入过队列
				if (node_next.flag_subturret == false)
				{ node_next.flag_subturret = true; queue.Enqueue(index_next); }
				
			}
		}//主循环结束
		if (index_next == list_nodes.Count)
			foreach (var item in list__turret_core_structures)//添加到炮塔核心结构
				if (struct__subturret_core.set_grids.IsSubsetOf(item.set_grids))
					item.list__subturret.Add(struct__subturret_core);
		builder_str__init_info.Append($"<info> bfs_subturret(): end\n\n");
	}

	//搜索活塞
	private void search_pistons(int index_start, SubTurretCoreStructure core)
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
				if (!list_adjacency[index_crt].ContainsKey(index_next) || !node_next.grid.CustomName.Contains(program.tag__elevation_pistons_gird))
					continue;//不是出边或不是特定网格就跳过
				if (node_next.flag_subturret == false) { node_next.flag_subturret = true; queue.Enqueue(index_next); }
			}
		}
		builder_str__init_info.Append($"<info> search_pistons(): end\n\n");
	}

	private void generate_init_info()
	{
		builder_str__init_info.Append($"<info> list_blocks.Count = {list_blocks.Count}\n");
		builder_str__init_info.Append($"<info> list_cameras.Count = {list_cameras.Count}\n");
		builder_str__init_info.Append($"<info> list_sensors.Count = {list_sensors.Count}\n");
		builder_str__init_info.Append($"<info> list_controllers.Count = {list_controllers.Count}\n");
		builder_str__init_info.Append($"<info> list_stators.Count = {list_stators.Count}\n");
		builder_str__init_info.Append($"<info> list_weapons.Count = {list_weapons.Count}\n");
		builder_str__init_info.Append($"<info> list__auto_turrets.Count = {list__auto_turrets.Count}\n");
		builder_str__init_info.Append($"<info> list_gyroscopes.Count = {list_gyros.Count}\n");
		builder_str__init_info.Append($"<info> list_timers.Count = {list_timers.Count}\n");
		builder_str__init_info.Append($"<info> list_displayers.Count = {list_displayers.Count}\n");
		builder_str__init_info.Append($"<info> list__other_blocks.Count = {list__other_blocks.Count}\n");

		//buinder_str__init_info.Append($"<info> list__other_blocks.Count = {list__other_blocks.Count}\n");
	}
}

/**************************************************
* 类 PIDCore
* PID算法核心, 封装了PID算法需要使用的相关内容
* 
* PID算法优化
* 比例系数:
* 
* 微分系数:
* 误差越大微分项系数越小( 微分项系数 = 微分项基准系数 / ln( error + 1 ) )
* 
* 积分系数:
* 某一侧输出到达极限时不进行同侧积分(积分抗饱和)
* 误差超过阈值时停止积分(积分分离)
* 
* 倍率映射 (K+(a/b))^((b-a)/a) K取[1,2], 当 a<b 时, 值大于1, 反之小于1
* 
**************************************************/
public class PIDCore
{
	public class CoeffVector//系数向量
	{
		public double p = 50; public double i = 0; public double d = 200; private Vector3D v;
		public CoeffVector() { ToString(); }
		public static bool TryParse(string str, ref CoeffVector value)
		{ if (Vector3D.TryParse(str, out value.v)) { value.p = value.v.X; value.i = value.v.Y; value.d = value.v.Z; return true; } else return false; }
		override public string ToString() { v.X = p; v.Y = i; v.Z = d; return v.ToString(); }
	}
	//向量 系数
	CoeffVector vector_coeff = null;
	//上限 输出上下限
	public static double limit_upper__output { get; } = 30; public static double limit_lower__output { get; } = -30;
	//阈值 积分分离阈值
	public static double threshold__integral_separation { get; } = Math.PI / 6;

	//标记 启用积分项
	public static bool flag__enable_integral_term { get; set; } = true;
	//标记 启用微分项
	public static bool flag__enable_derivative_term { get; set; } = true;
	//标记 启用积分抗饱和
	public static bool flag__enable_integral_anti_windup { get; set; } = true;
	//标记 启用积分分离
	public static bool flag__enable_integral_separation { get; set; } = true;

	//调用计数
	public int count_invoke { get; private set; } = 0;

	Program program;

	//积分值
	private double integral = 0.0;
	//误差值 上一次误差
	private double error_last = 0.0;
	//输出值 上一次的输出值
	private double output_last = 0.0;

	//构造函数
	public PIDCore(Program _program, CoeffVector vec)
	{
		this.program = _program;
		this.vector_coeff = vec;
		return;
	}

	//计算输出
	public double cal_output(double error)
	{
		double derivative = (PIDCore.flag__enable_derivative_term) ? (error - error_last) : 0.0;
		derivative = derivative * derivative;//微分取平方
		if (PIDCore.flag__enable_integral_term)
			if (flag__enable_integral_separation ? (error < threshold__integral_separation && error > -threshold__integral_separation) : true)
				if (flag__enable_integral_anti_windup ? ((output_last > limit_upper__output && error < 0) || (output_last < limit_lower__output && error > 0)) : true)
					integral += error;
		++count_invoke; error_last = error;
		if (vector_coeff != null)
			output_last = (vector_coeff.p * error + vector_coeff.i * integral + vector_coeff.d * derivative);
		//if (Double.IsNaN(output_last)) program.string_builder__test_info.Append("output is NaN\n");
		//if (Double.IsInfinity(output_last)) program.string_builder__test_info.Append("output is INF\n");
		if (output_last > limit_upper__output)
			return limit_upper__output;
		else if (output_last < limit_lower__output)
			return limit_lower__output;
		return output_last;
	}

}

#endregion

#region 配置通用

/***************************************************************************************************
* 类 自定义数据配置
* 自定义数据配置(下简称CD配置)使用目标方块的自定义数据来进行脚本配置
* 支持动态配置, 功能强大
* 
***************************************************************************************************/

//管理对象
public class CustomDataConfig
{
	//映射表 配置项集合
	Dictionary<string, CustomDataConfigSet> dict__config_sets = new Dictionary<string, CustomDataConfigSet>();
	//映射表 字符串内容
	Dictionary<string, List<string>> dict__str_contents = new Dictionary<string, List<string>>();
	//字符串构建器 数据
	public StringBuilder string_builder__data { get; private set; } = new StringBuilder();
	//字符串构建器 错误信息
	public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();

	//终端方块 CD配置的目标方块
	public IMyTerminalBlock block_target { get; private set; }

	//标记 配置中发现错误(存在错误时不会覆盖写入)
	public bool flag__config_error { get; private set; } = false;

	public CustomDataConfig(IMyTerminalBlock block_target)
	{
		this.block_target = block_target;
	}

	//初始化配置
	public void init_config()
	{
		parse_custom_data();//解析自定义数据
		if (!flag__config_error)//检查CD配置是否存在错误
			write_to_block_custom_data();//写入自定义数据
	}

	//添加配置集
	public bool add_config_set(CustomDataConfigSet set)
	{
		if (dict__config_sets.ContainsKey(set.title__config_set))
			return false;
		dict__config_sets.Add(set.title__config_set, set);
		return true;
	}

	//解析CD(拆分)
	public void parse_custom_data()
	{
		//以换行符拆分
		string[] array_lines = block_target.CustomData.Split('\n');
		string pattern = "#{3,} (.+) #{3,}";//正则表达式
		var regex = new System.Text.RegularExpressions.Regex(pattern);
		string title_crt = "";
		foreach (var line in array_lines)
		{
			var match = regex.Match(line);//正则匹配
			if (line.Length == 0) continue;
			else if (match.Success)
				dict__str_contents[title_crt = match.Groups[1].ToString()] = new List<string>();
			else if (dict__str_contents.ContainsKey(title_crt))
				dict__str_contents[title_crt].Add(line);
			else
			{
				string_builder__error_info.Append($"<error> illegal CD config data: \n{line}\n");
				flag__config_error = true; break;
			}
		}
		foreach (var pair in dict__str_contents)
		{
			if (dict__config_sets.ContainsKey(pair.Key))
				if (!dict__config_sets[pair.Key].parse_string_data(pair.Value))
				{
					string_builder__error_info.Append($"<error> illegal config item in config set: [{pair.Key}]\n{dict__config_sets[pair.Key].string_builder__error_info}\n");
					flag__config_error = true; break;
				}
		}
	}

	//写入方块CD
	public void write_to_block_custom_data()
	{
		foreach (var item in dict__config_sets)
		{
			item.Value.generate_string_data();
			string_builder__data.Append(item.Value.string_builder__data);
		}
		block_target.CustomData = string_builder__data.ToString();
	}
}

public class CustomDataConfigSet
{
	//类 配置项指针
	public class ConfigItemReference
	{
		//读委托
		public Func<object> get { get; private set; }

		//写委托
		public Action<object> set { get; private set; }

		//构造函数 传递委托(委托类似于函数指针, 用于像指针那样读写变量)
		public ConfigItemReference(Func<object> _getter, Action<object> _setter)
		{
			get = _getter; set = _setter;
		}
	}

	//配置集标题
	public string title__config_set { get; private set; }

	//字典 配置项字典
	Dictionary<string, ConfigItemReference> dict__config_items = new Dictionary<string, ConfigItemReference>();

	//字符串构建器 数据
	public StringBuilder string_builder__data { get; private set; } = new StringBuilder();
	//字符串构建器 错误信息
	public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();

	//标记 配置中发现错误(存在错误时不会覆盖写入)
	public bool flag__config_error { get; private set; } = false;

	//构造函数
	public CustomDataConfigSet(string title_config = "SCRIPT CONFIGURATION")
	{
		this.title__config_set = title_config;
	}

	//添加配置项
	public bool add_config_item(string name_config_item, Func<object> getter, Action<object> setter)
	{
		if (dict__config_items.ContainsKey(name_config_item))
			return false;
		dict__config_items.Add(name_config_item, new ConfigItemReference(getter, setter));
		return true;
	}

	//添加分割线
	public bool add_line(string str_title)
	{
		//检查是否已经包含此配置项目
		if (dict__config_items.ContainsKey(str_title))
			return false;
		//添加到字典
		dict__config_items.Add(str_title, null);
		return true;
	}

	//更新配置项的值
	//public bool update_config_item_value(string name__config_item, object value__config_item, bool flag_rewrite = true)
	//{
	//	if (!dict__config_items.ContainsKey(name__config_item))
	//		return false;

	//	dict__config_items[name__config_item].set(value__config_item);
	//	//重新写入
	//	if (flag_rewrite)
	//		write_to_block_custom_data();
	//	return true;
	//}

	//解析字符串
	public bool parse_string_data(List<string> content)
	{
		int count = 0;

		foreach (var line in content)
		{
			++count;
			if (line.Length == 0)
				continue;//跳过空行

			if (line.StartsWith("---"))
				continue;

			//以等号拆分
			string[] pair = line.Split('=');
			//检查
			if (pair.Length != 2)
			{
				string_builder__error_info.Append($"<error> \"{line}\"(at line {count}) is not a legal config item");
				flag__config_error = true;
				continue;//跳过配置错误的行
			}

			//去除多余空格
			string name__config_item = pair[0].Trim();
			string str_value__config_item = pair[1].Trim();

			ConfigItemReference reference;
			//尝试获取
			if (!dict__config_items.TryGetValue(name__config_item, out reference))
				continue;//不包含值, 跳过

			//包含值, 需要解析并更新
			var value = reference.get();//获取值
			if (parse_string(str_value__config_item, ref value))
				//成功解析字符串, 更新数值
				dict__config_items[name__config_item].set(value);
			else
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
		string_builder__data.Append($"########## {title__config_set} ##########\n");
		foreach (var pair in dict__config_items)
		{
			if (pair.Value != null)
				string_builder__data.Append($"{pair.Key} = {pair.Value.get()}\n");
			else
			{
				if (count != 0)string_builder__data.Append("\n");
				string_builder__data.Append($"---------- {pair.Key} ----------\n");
			}
			++count;
		}
		string_builder__data.Append("\n\n");
	}

	//解析字符串值
	private bool parse_string(string str, ref object v)
	{
		if (v is bool)
		{
			bool value_parsed;
			if (bool.TryParse(str, out value_parsed))
			{
				v = value_parsed;
				return true;
			}
		}
		else if (v is float)
		{
			float value_parsed;
			if (float.TryParse(str, out value_parsed))
			{
				v = value_parsed;
				return true;
			}
		}
		else if (v is double)
		{
			double value_parsed;
			if (double.TryParse(str, out value_parsed))
			{
				v = value_parsed;
				return true;
			}
		}
		else if (v is int)
		{
			int value_parsed;
			if (int.TryParse(str, out value_parsed))
			{
				v = value_parsed;
				return true;
			}
		}
		else if (v is Vector3D)
		{
			Vector3D value_parsed;
			if (Vector3D.TryParse(str, out value_parsed))
			{
				v = value_parsed;
				return true;
			}
		}
		else if (v is string)
		{
			v = str;
			return true;
		}
		else if (v is PIDCore.CoeffVector)
		{
			PIDCore.CoeffVector vector = (PIDCore.CoeffVector)v;
			if (PIDCore.CoeffVector.TryParse(str, ref vector))
			{
				v = vector;
				return true;
			}
		}
		return false;
	}

}

#endregion