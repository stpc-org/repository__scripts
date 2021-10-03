/***************************************************************************************************                          
* 
* ### Advanced Multiple Cannon Control System ###
* ### AMCCS | 高级多联装火炮控制系统脚本 ---- ###
* ### Version 0.1.7 | by SiriusZ-BOTTLE ----- ###
* ### STPC旗下SCP脚本工作室开发 欢迎加入STPC  ###
* ### STPC主群群号:320461590 我们欢迎新朋友-- ###
* 
* 脚本使用说明书: (也可进群咨询使用方法)
* https://github.com/stpc-org/repository_scripts/tree/main/release/AMCCS
* 
***************************************************************************************************/

//使用脚本时确保下面一行代码被注释
#define DEVELOP

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

namespace AMCCS_DEV
{
	class Program:MyGridProgram
	{
#endif

		#region 脚本字段

		//字符串 脚本版本号
		readonly string str__script_version = "AMCCS V0.1.7 ";
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

		//枚举变量 脚本运行模式(默认常规模式)
		ScriptMode mode_script = ScriptMode.Regular;
		//枚举变量 默认组间射击模式
		FireMode mode_fire__intra_group_default = FireMode.Round;
		//枚举变量 默认组内射击模式
		FireMode mode_fire__inter_group_default = FireMode.Round;

		//索引 开头编组(本脚本管理的火炮起始编号, 含)
		int index__cannon_begin = 0;
		//索引 末尾编组(本脚本管理的火炮末尾编号, 含)
		int index__cannon_end = 0;
		//数量 编组中的火炮
		int number__cannons_in_group = 0;

		//标签 火炮活塞元件编组
		string tag__cannon_pistons_group = "Pistons_cannon_#";
		//标签 火炮释放元件编组
		string tag__cannon_releasers_group = "Releasers_cannon_#";
		//标签 火炮分离元件编组
		string tag__cannon_detachers_group = "Detachers_cannon_#";
		//标签 火炮弹头编组
		string tag__cannon_shell_warheads_group = "Warheads_cannon_shell_#";
		//标签 火炮分离部件网格定位器
		string tag__cannon_detach_part_grid_locator = "Locator_cannon_detach_part_grid_#";
		//标签 火炮焊接器编组
		string tag__cannon_shell_welders_group = "Welders_cannon_#";
		//标签 火炮垂直关节元件编组
		string tag__cannon_vertical_joints_group = "Joints_cannon_vertical_#";
		//标签 火炮编组射击指示器编组
		string tag__cannon_group_fire_indicators_group = "Indicators_group_fire_#";
		//标签 后缀 火炮状态指示器
		string tag__postfix_cannon_status_indicator = "_indicator";
		//标签 接口定时器0
		string tag__custom_interface_timer_0 = "Timer_cannon_custom_0_#";
		//标签 接口定时器1
		string tag__custom_interface_timer_1 = "Timer_cannon_custom_1_#";
		//标签 信息显示LCD编组
		string tag__info_display_LCD_group = "LCDs_cannon_ctrl_info_display";
		//标签 火炮破速限器编组
		string tag__cannon_speed_limit_breaker = "SpeedLimitBreakers_cannon_#";
		//标签 火炮次要活塞元件编组
		string tag__cannon_minor_pistons_group = "Pistons_cannon_minor_#";
		//标签 火炮固定器编组
		string tag__cannon_fixators_group = "Fixators_cannon_#";

		//标记(全局) 是否 自动激活炮弹
		bool flag__auto_activate_shell = true;
		//标记(全局) 是否 自动开启弹头倒计时
		bool flag__auto_start_warhead_countdown = true;
		//标记(全局) 是否 自动切换焊接器开关状态
		bool flag__auto_toggle_welders_onoff = true;
		//标记(全局) 是否 自动切换垂直关节元件锁定状态
		bool flag__auto_toggle_vertical_joints_lock = true;
		//标记(全局) 是否 自动触发用户接口定时器0
		bool flag__auto_trigger_custom_interface_timer_0 = true;
		//标记(全局) 是否 自动触发用户接口定时器1
		bool flag__auto_trigger_custom_interface_timer_1 = true;
		//标记(全局) 是否 自动检查
		bool flag__auto_check = true;
		//标记(全局) 是否 自动重载
		bool flag__auto_reload = true;
		//标记(全局) 是否 脚本在初始化之后自动将所有火炮重载一次
		bool flag__reload_once_after_script_initialization = true;
		//标记(全局) 是否 启用二段蓄力模式(此项不开启则有关项全部隐藏)
		bool flag__enable_two_stage_mode = false;
		//标记(全局) 是否 同步的次要活塞
		bool flag__synchronized_minor_pistons = false;
		//标记(全局) 是否 启用主动断开炮弹连接功能
		bool flag__enable_shell_disconnection = false;

		//周期 自动检查(默认每秒检查一次)
		int period__auto_check = 60;
		//周期 自动射击(默认每秒射击一次)
		int period__auto_fire = 60;
		//周期 更新输出(默认每秒3次)
		int period__update_output = 20;

		//时刻 释放(默认无延迟)
		int delay_release = 1;
		//时刻 分离开始(默认无时刻)
		int delay__detach_begin = 1;
		//时刻 分离(默认2帧)
		int delay_detach = 2;
		//时刻 分离结束(默认10帧)
		int delay__detach_end = 10;
		//时刻 尝试激活炮弹
		int delay__try_activate_shell = 5;
		//时刻 尝试断开炮弹连接
		int delay__try_disconnect_shell = 10;
		//时刻 伸展(取值 [3, 5] 内比较合适)
		int delay__pistons_extend = 5;
		//时刻 固定(默认35帧后附着)
		int delay_attach = 35;
		//时刻 收缩(默认150帧后收缩)
		int delay__pistons_retract = 150;
		//时刻 定时器 用户自定义接口0(默认无延迟)
		int delay__custom_interface_timer_0 = 1;
		//时刻 定时器 用户自定义接口1(默认180帧)
		int delay__custom_interface_timer_1 = 180;
		//时刻 完成装填(默认180帧)
		int delay__done_loading = 180;

		//时刻 第一次重载
		int delay__first_reload = 30;
		//时刻 暂停时长
		int delay_pausing = 20;

		//时刻 关闭固定器 (测试的稳定值在 [3, 5] 左右)
		int delay__disable_fixators = 3;
		//时刻 次要活塞伸展 (测试的稳定值在 1 左右)
		int delay__minor_pistons_extend = 1;
		//时刻 开启固定器 (错位之后开启就行, 无其它要求)
		int delay__enable_fixators = 60;
		//时刻 次要活塞收缩 (测试的稳定值在 70 左右)
		int delay__minor_pistons_retract = 80;


		//距离 弹头安全锁(默认20米)
		double distance__warhead_savety_lock = 20.0;
		//数量 倒计时秒数
		double number__warhead_countdown_seconds = 30.0;
		//时间 断开炮弹连接耗时
		int time__disconnecting_shell = 10;

		//以上是脚本配置字段

		//列表 活塞炮
		List<PistonCannon> list__piston_cannons = new List<PistonCannon>();
		//列表 火炮编组
		List<CannonGroup> list__cannon_groups = new List<CannonGroup>();
		//列表 显示器
		List<IMyTextPanel> list_displayers = new List<IMyTextPanel>();
		//列表 显示器提供者 
		List<IMyTextSurfaceProvider> list__displayers_providers = new List<IMyTextSurfaceProvider>();
		//列表 显示单元
		List<DisplayUnit> list__display_units = new List<DisplayUnit>();

		//索引 上一次射击的编组
		int index__group_last_fire = -1;

		//延迟 自动射击前的延迟
		int delay__before_auto_fire = 5;

		//枚举变量 组间射击模式(默认齐射)
		FireMode mode_fire__inter_group = FireMode.Round;
		//枚举变量 组内射击模式(默认轮射)
		FireMode mode_fire__intra_group = FireMode.Round;

		//计数 运行命令数
		long count__run_cmd = 0;
		//计数 更新
		long count_update = 0;
		//计数 射击
		long count_fire = 0;
		//索引 当前字符图案
		long index__crt_char_pattern = 0;
		//次数 距离下一次 字符图案更新
		long times__before_next_char_pattern_update = 0;
		//次数 距离下一次 自动射击
		long times__before_next_auto_fire = 0;
		//次数 距离下一次 更新输出
		long times__before_next_output_update = 0;

		//标记(全局) 是否 自动射击
		bool flag__auto_fire = false;

		//字符串构建器 脚本信息
		StringBuilder string_builder__script_info = new StringBuilder();
		//字符串构建器 测试信息
		StringBuilder string_builder__test_info = new StringBuilder();
		//脚本配置
		CustomDataConfig config_script;
		//配置集合 脚本
		CustomDataConfigSet config_set__script;
		//配置集合 二段式火炮
		CustomDataConfigSet config_set__t;

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
		void Main(string str_arg,UpdateType type_update)
		{
			switch(type_update)
			{
				case UpdateType.Terminal:
				case UpdateType.Trigger:
				case UpdateType.Script:
					{
						run_command(str_arg);
					}
					break;
				case UpdateType.Update1:
				case UpdateType.Update10:
				case UpdateType.Update100:
					{
						update_script();
					}
					break;
			}
		}

		#endregion

		#region 成员函数

		//脚本更新
		void update_script()
		{
			if(mode_script==ScriptMode.WeaponSync)
			{
				//武器同步模式
				//检查所有群组
				flag__auto_fire=false;
				foreach(var item in list__cannon_groups)
					if(item.check_and_update_weapon_synchronization_status())
						flag__auto_fire=true;
			}

			//自动射击
			if(flag__auto_fire)
			{
				if(times__before_next_auto_fire==0)
				{
					times__before_next_auto_fire=period__auto_fire;
					fire();
				}
				--times__before_next_auto_fire;
			}

			//更新所有火炮
			foreach(var item in list__piston_cannons)
				item.update();
			//信息显示
			display_info();
			//更新次数+1
			++count_update;
			return;
		}

		//执行指令
		void run_command(string str_arg)
		{
			var cmd = split_string(str_arg);//空格拆分
			++count__run_cmd;//更新计数
			if(cmd.Count==0)
				fire();
			else if(cmd.Count==1)
			{
				switch(cmd[0])//检查命令
				{
					case "":
					case "fire"://开火
						fire();
						break;
					case "fire_SS"://开火
						fire(FireMode.Salvo,FireMode.Salvo);
						break;
					case "fire_SR"://开火
						fire(FireMode.Salvo,FireMode.Round);
						break;
					case "fire_RS"://开火
						fire(FireMode.Round,FireMode.Salvo);
						break;
					case "fire_RR"://开火
						fire(FireMode.Round,FireMode.Round);
						break;
					case "toggle_inter_group_fire_mode"://切换组间射击模式
						{
							if(mode_fire__inter_group==FireMode.Salvo)
								mode_fire__inter_group=FireMode.Round;
							else
								mode_fire__inter_group=FireMode.Salvo;
						}
						break;
					case "toggle_intra_group_fire_mode"://切换组内射击模式
						{
							if(mode_fire__intra_group==FireMode.Salvo)
								mode_fire__intra_group=FireMode.Round;
							else
								mode_fire__intra_group=FireMode.Salvo;
							foreach(var item in list__cannon_groups)
								item.set_group_fire_mode(mode_fire__intra_group);
						}
						break;
					case "toggle_auto_fire_onoff":
						{
							flag__auto_fire=!flag__auto_fire;
							//设置自动射击前的延迟
							times__before_next_auto_fire=delay__before_auto_fire;
						}
						break;
					case "check_cannons_state":
						{
							//更新所有火炮
							foreach(var item in list__piston_cannons)
								item.check_once_immediately();
						}
						break;
				}
			}
			else
			{
				int index_group = 0;
				int.TryParse(cmd[1],out index_group);
				switch(cmd[0])//检查命令
				{
					case "fire":
						fire_specific_group(index_group);
						break;
					case "fire_S":
						fire_specific_group(index_group,FireMode.Salvo);
						break;
					case "fire_R":
						fire_specific_group(index_group,FireMode.Round);
						break;
				}
			}
		}

		//输出信息 输出信息到编程块终端和LCD
		void display_info()
		{
			if(times__before_next_output_update!=0)
			{
				--times__before_next_output_update;
				return;
			}
			else
				times__before_next_output_update=period__update_output;
			--times__before_next_output_update;

			//清空
			string_builder__script_info.Clear();
			string_builder__script_info.Append(
				"<script> "+str__script_version+array__runtime_chars[index__crt_char_pattern]
				+"\n<mode_script>"+mode_script
				+"\n<mode_fire__inter_group>"+mode_fire__inter_group
				+"\n<mode_fire__intra_group>"+mode_fire__intra_group
				+"\n<count_update> "+count_update
				+"\n<count_trigger> "+count__run_cmd
				+"\n<count_fire> "+count_fire
				+"\n<flag__auto_fire> "+flag__auto_fire
				+"\n<count_groups> total "+list__cannon_groups.Count
				+"\n<count_cannons> total "+list__piston_cannons.Count
				);
			Echo(string_builder__script_info.ToString());

			//更新动态字符图案
			if(times__before_next_char_pattern_update==0)
			{
				times__before_next_char_pattern_update=1;
				++index__crt_char_pattern;
				if(index__crt_char_pattern>=array__runtime_chars.Length)
					index__crt_char_pattern=0;
			}
			--times__before_next_char_pattern_update;

			foreach(var item in list__cannon_groups)
			{
				Echo(item.get_group_info());
			}

			//遍历显示单元
			foreach(var item in list__display_units)
			{
				if(item.flag_graphic)
				{
					//图形化显示

					if(item.displayer.ContentType!=ContentType.SCRIPT)
						continue;

					switch(item.mode_display)
					{
						case DisplayUnit.DisplayMode.General:
							draw_illegal_lcd_custom_data_hint(item.displayer);
							break;
						case DisplayUnit.DisplayMode.SingleCannon:
							draw_cannons_state(item.displayer,item.index_begin,item.index_begin);
							break;
						case DisplayUnit.DisplayMode.MultipleCannon:
							draw_cannons_state(item.displayer,item.index_begin,item.index_end);
							break;
						case DisplayUnit.DisplayMode.SingleGroup:
							draw_cannons_state(item.displayer,
								list__cannon_groups[item.index_begin].get__index_begin(),
								list__cannon_groups[item.index_begin].get__index_end());
							break;
						case DisplayUnit.DisplayMode.MultipleGroup:
							{

							}
							break;
						case DisplayUnit.DisplayMode.None:
							draw_illegal_lcd_custom_data_hint(item.displayer);
							break;
					}
				}
				else
				{
					//非图形化显示

					if(item.displayer.ContentType!=ContentType.TEXT_AND_IMAGE)
						continue;

					switch(item.mode_display)
					{
						case DisplayUnit.DisplayMode.General:
							item.displayer.WriteText(string_builder__script_info);
							break;
						case DisplayUnit.DisplayMode.SingleCannon:
							item.displayer.WriteText(
								list__piston_cannons[item.index_begin].get_cannon_LCD_info());
							break;
						case DisplayUnit.DisplayMode.MultipleCannon:
							{
								StringBuilder sb_temp = new StringBuilder();
								for(var i = item.index_begin;i<=item.index_end;++i)
									sb_temp.Append(list__piston_cannons[i].get_cannon_simple_info()+"\n");
								item.displayer.WriteText(sb_temp);
							}
							break;
						case DisplayUnit.DisplayMode.SingleGroup:
							item.displayer.WriteText(list__cannon_groups[item.index_begin].get_group_LCD_info());
							break;
						case DisplayUnit.DisplayMode.MultipleGroup:
							{
								StringBuilder sb_temp = new StringBuilder();
								for(var i = item.index_begin;i<=item.index_end;++i)
									sb_temp.Append(list__cannon_groups[i].get_group_LCD_info()+"\n");
								item.displayer.WriteText(sb_temp);
							}
							break;
						case DisplayUnit.DisplayMode.None:
							item.displayer.WriteText("<warning> illegal custom data in this LCD\n<by> script AMCCS");
							break;
					}
				}
			}

			//显示测试信息
			//Echo(string_builder__test_info.ToString());

			return;
		}

		//绘制 提示 LCD的自定义数据非法
		void draw_illegal_lcd_custom_data_hint(IMyTextSurface surface)
		{
			//帧缓冲区
			MySpriteDrawFrame frame = surface.DrawFrame();
			//获取显示器大小
			Vector2 size = surface.SurfaceSize;

			//绘图元素 红色圆环
			MySprite element__red_annulus = new MySprite()
			{
				Type=SpriteType.TEXTURE,
				Data="CircleHollow",
				Position=size*(new Vector2(0.5f,0.4f)),
				Size=size*0.6f,
				Color=new Color(255,0,0),
				Alignment=TextAlignment.CENTER,
			};
			frame.Add(element__red_annulus);

			//绘图元素 红色叉
			MySprite element__red_cross = new MySprite()
			{
				Type=SpriteType.TEXTURE,
				Data="Cross",
				Position=size*(new Vector2(0.5f,0.4f)),
				Size=size*0.4f,
				Color=new Color(255,0,0),
				Alignment=TextAlignment.CENTER,
			};
			frame.Add(element__red_cross);

			//白色文字
			MySprite element__text = new MySprite()
			{
				Type=SpriteType.TEXT,
				Data="<warning> illegal custom data in this LCD\n<by> script AMCCS",
				Position=size*(new Vector2(0.5f,0.8f)),
				Color=Color.White,
				Alignment=TextAlignment.CENTER,
				FontId="White",
				RotationOrScale=1.0f,
			};
			frame.Add(element__text);

			//刷新缓冲区
			frame.Dispose();

		}

		//绘制 火炮状态
		void draw_cannons_state(IMyTextSurface surface,int index_begin,int index_end)
		{
			//帧缓冲区
			MySpriteDrawFrame frame = surface.DrawFrame();

			//大小 显示屏
			Vector2 size = surface.SurfaceSize;

			Vector2 offset_viewpoint = (surface.TextureSize-surface.SurfaceSize)/2f;
			//偏移量 条
			Vector2 offset_bar = size;
			//偏移量 编号
			Vector2 offset_no = size;
			//大小 条形
			Vector2 size_rect = size;

			//增量 偏移量y轴方向
			float increment__offset_y = size.Y*0.15f;

			size_rect.X*=0.7f;
			size_rect.Y*=0.1f;

			offset_bar.X*=0.25f;
			offset_bar.Y*=0.10f;

			offset_no.X*=0.05f;
			offset_no.Y*=0.02f;

			offset_bar+=offset_viewpoint;
			offset_no+=offset_viewpoint;

			for(int index = index_begin;index<=index_end;++index)
			{
				MySprite element_bar = new MySprite()
				{
					Type=SpriteType.TEXTURE,
					Data="SquareSimple",
					Position=offset_bar,
					Size=size_rect,
					Color=Color.White,
					FontId="Monospace",
					Alignment=TextAlignment.LEFT,
				};
				//根据状态调整颜色
				switch(list__piston_cannons[index].status_cannon)
				{
					case PistonCannon.CannonStatus.Ready://就绪
						element_bar.Color=Color.Green;//绿色
						break;
					case PistonCannon.CannonStatus.Loading://装填中
						element_bar.Color=Color.White;//白色
						element_bar.Size=size_rect*(new Vector2((float)list__piston_cannons[index].count_status/delay__done_loading,1.0f));
						break;
					case PistonCannon.CannonStatus.BrokenDown://故障
						element_bar.Color=Color.Red;//红色
						break;
					case PistonCannon.CannonStatus.Invalid://不可用
						element_bar.Color=Color.Yellow;//黄色
						break;
				}
				frame.Add(element_bar);

				MySprite element_no = new MySprite()
				{
					Type=SpriteType.TEXT,
					Data="#"+(index+index__cannon_begin),
					Position=offset_no,
					Color=Color.White,
					Alignment=TextAlignment.LEFT,
					FontId="White",
					//RotationOrScale=2.5f,//固定大小
					RotationOrScale=0.005f*size.Y,//大小随屏幕大小变化
				};
				frame.Add(element_no);

				//向下偏移
				offset_bar.Y+=increment__offset_y;
				offset_no.Y+=increment__offset_y;
			}
			frame.Dispose();
			return;
		}

		//射击控制 (总)
		void fire(FireMode mode_0 = FireMode.None,FireMode mode_1 = FireMode.None)
		{
			var mode_temp = mode_0==FireMode.None ? mode_fire__inter_group : mode_0;
			switch(mode_temp)
			{
				case FireMode.Salvo:
					fire_salvo(mode_1);
					break;
				case FireMode.Round:
					fire_round(mode_1);
					break;
			}
		}

		//齐射
		void fire_salvo(FireMode mode)
		{
			//所有编组尝试射击
			foreach(var item in list__cannon_groups)
				item.fire(mode);
			return;
		}

		//轮射
		void fire_round(FireMode mode)
		{
			int index = -1;

			//从上一次开炮的位置往后查找到末尾
			int index__last_next = index__group_last_fire+1;
			for(index=index__last_next;index<list__cannon_groups.Count;++index)
			{
				//是否成功执行射击
				if(list__cannon_groups[index].fire(mode)!=0)
					break;
			}
			if(index==list__cannon_groups.Count)
			{
				//没有找到
				//从开头重新查找到上次开炮的位置
				for(index=0;index<index__last_next;++index)
				{
					//是否成功执行射击
					if(list__cannon_groups[index].fire(mode)!=0)
						break;
				}
			}
			return;
		}

		//特定编组射击
		void fire_specific_group(int index_group = 0,FireMode mode = FireMode.None)
		{
			if(index_group>-1&&index_group<list__cannon_groups.Count)
				list__cannon_groups[index_group].fire(mode);
		}

		//初始化脚本
		void init_script()
		{
			init_script_config();//初始化脚本配置
			string str_error = check_config();
			//检查配置合法性
			if(str_error!=null) Echo(str_error);

			config_script.init_config();//初始化配置
			Echo(config_script.string_builder__error_info.ToString());

			//初始模式
			mode_fire__intra_group=mode_fire__intra_group_default;
			mode_fire__inter_group=mode_fire__inter_group_default;

			//计算编组数量
			int num_groups = 1, num_cannons = index__cannon_end-index__cannon_begin+1;
			int num__cannons_in_group = number__cannons_in_group;
			if(num__cannons_in_group!=0)
			{
				if(num_cannons%num__cannons_in_group==0)
					num_groups=num_cannons/num__cannons_in_group;
				else
					num_groups=num_cannons/num__cannons_in_group+1;
			}
			else
				num__cannons_in_group=num_cannons;
			//创建火炮编组对象 并添加到列表
			for(int i = 0;i<num_groups;++i)
			{
				CannonGroup ptr = new CannonGroup(this,i);
				list__cannon_groups.Add(ptr);
			}
			//创建火炮对象 并添加到列表
			for(int i = index__cannon_begin, index_group = 0;i<=index__cannon_end;++i)
			{
				index_group=(i-index__cannon_begin)/num__cannons_in_group;
				PistonCannon ptr_temp = new PistonCannon
					(this,i,list__cannon_groups[index_group]);
				//添加到列表和编组中
				list__piston_cannons.Add(ptr_temp);
				list__cannon_groups[index_group].add_cannon(ptr_temp);
			}

			//初始化显示单元
			init_script_display_units();

			//变量初始化
			times__before_next_auto_fire=delay__before_auto_fire;

			if(str_error==null&&!config_script.flag__config_error)//检查是否出现错误
				Runtime.UpdateFrequency=UpdateFrequency.Update1;//设置执行频率 每1帧一次
		}

		//检查数值
		bool check_value(int value) => value>0&&value<1001;

		//拆分字符串
		List<string> split_string(string str)
		{
			List<string> list = new List<string>(str.Split(new string[] { " " },StringSplitOptions.RemoveEmptyEntries));
			return list;
		}

		List<string> split_string_2(string str)
		{
			List<string> list = new List<string>(str.Split(new string[] { "\n" },StringSplitOptions.RemoveEmptyEntries));
			return list;
		}

		//初始化脚本显示单元
		void init_script_display_units()
		{
			//获取LCD编组
			IMyBlockGroup group_temp = GridTerminalSystem.GetBlockGroupWithName(tag__info_display_LCD_group);

			if(group_temp!=null)
			{
				//获取编组中的显示器和显示器提供者
				group_temp.GetBlocksOfType<IMyTextPanel>(list_displayers);
				group_temp.GetBlocksOfType<IMyTextSurfaceProvider>(list__displayers_providers);
			}

			Dictionary<IMyTextSurface,List<string>> dict = new Dictionary<IMyTextSurface,List<string>>();

			foreach(var item in list_displayers)
				dict[item as IMyTextSurface]=split_string(item.CustomData);

			foreach(var item in list__displayers_providers)
			{
				//以换行拆分
				var list_lines = split_string_2((item as IMyTerminalBlock).CustomData);

				foreach(var line in list_lines)
				{
					//拆分行
					var list_str = split_string(line);
					int index = 0;

					if((!int.TryParse(list_str[0],out index))||index>=item.SurfaceCount||index<0)
						continue;//解析失败或者索引越界, 跳过
					list_str.RemoveAt(0);
					dict[item.GetSurface(index)]=list_str;
				}
			}

			//遍历显示器
			foreach(var pair in dict)
			{
				//拆分显示器的用户数据
				List<string> list_str = pair.Value;
				bool flag_illegal = false;
				int offset = 0;

				DisplayUnit unit = new DisplayUnit(pair.Key);

				if(list_str.Count==0)
					unit.mode_display=DisplayUnit.DisplayMode.General;//用户数据为空
				else
				{
					//以 "graphic" 结尾
					if(list_str[list_str.Count-1].Equals("graphic"))
					{
						offset=1;
						unit.flag_graphic=true;
					}

					//用户数据不为空
					switch(list_str[0])
					{
						case "graphic":
						case "general":
							unit.mode_display=DisplayUnit.DisplayMode.General;
							break;
						case "cannon":
							{
								if(list_str.Count==2+offset)//单个
								{
									int index = 0;
									if(!int.TryParse(list_str[1],out index))
									{
										flag_illegal=true;
										break;
									}
									//边界检查
									if(index<index__cannon_begin||index>index__cannon_end)
									{
										flag_illegal=true;
										break;
									}
									unit.index_begin=index-index__cannon_begin;
									unit.mode_display=DisplayUnit.DisplayMode.SingleCannon;
								}
								else if(list_str.Count==3+offset)//多个
								{
									int index_begin = 0, index_end = 0;
									if(!int.TryParse(list_str[1],out index_begin))
									{
										flag_illegal=true;
										break;
									}
									if(!int.TryParse(list_str[2],out index_end))
									{
										flag_illegal=true;
										break;
									}
									//边界检查
									if(index_begin<index__cannon_begin||index_begin>index__cannon_end)
									{
										flag_illegal=true;
										break;
									}
									if(index_end<index__cannon_begin||index_end>index__cannon_end)
									{
										flag_illegal=true;
										break;
									}
									unit.index_begin=index_begin-index__cannon_begin;
									unit.index_end=index_end-index__cannon_begin;
									unit.mode_display=DisplayUnit.DisplayMode.MultipleCannon;
								}
								else
									flag_illegal=true;
							}
							break;
						case "group":
							{
								if(list_str.Count==2+offset)
								{
									int index = 0;
									if(!int.TryParse(list_str[1],out index))
									{
										flag_illegal=true;
										break;
									}
									//边界检查
									if(index<0||index>=list__cannon_groups.Count)
									{
										flag_illegal=true;
										break;
									}
									unit.index_begin=index;
									unit.mode_display=DisplayUnit.DisplayMode.SingleGroup;
								}
								else if(list_str.Count==3+offset)
								{
									int index_begin = 0, index_end = 0;
									if(!int.TryParse(list_str[1],out index_begin))
									{
										flag_illegal=true;
										break;
									}
									if(!int.TryParse(list_str[2],out index_end))
									{
										flag_illegal=true;
										break;
									}
									//边界检查
									if(index_begin<0||index_begin>=list__cannon_groups.Count)
									{
										flag_illegal=true;
										break;
									}
									if(index_end<0||index_end>=list__cannon_groups.Count)
									{
										flag_illegal=true;
										break;
									}
									unit.index_begin=index_begin;
									unit.index_end=index_end;
									unit.mode_display=DisplayUnit.DisplayMode.MultipleGroup;
								}
								else
									flag_illegal=true;
							}
							break;
						default:
							unit.mode_display=DisplayUnit.DisplayMode.None;
							break;
					}
				}

				if(flag_illegal)
					unit.mode_display=DisplayUnit.DisplayMode.None;

				//自动设置LCD的显示模式
				if(unit.flag_graphic)
				{
					pair.Key.ContentType=ContentType.SCRIPT;//设置为 脚本模式
					pair.Key.Script="";//脚本设为 None
					pair.Key.ScriptBackgroundColor=Color.Black;//黑色背景色
				}
				else
					pair.Key.ContentType=ContentType.TEXT_AND_IMAGE;//设置为 文字与图片模式

				//添加到列表
				list__display_units.Add(unit);
			}
		}

		//初始化脚本配置
		void init_script_config()
		{
			//脚本配置实例
			config_script=new CustomDataConfig(Me);

			config_set__script=new CustomDataConfigSet("SCRIPT CONFIGURATION");
			config_set__t=new CustomDataConfigSet("TWO STAGE CANNON");

			//添加配置项
			config_set__script.add_line("MODE OF THE SCRIPT");
			config_set__script.add_config_item(nameof(mode_script),() => mode_script,x => { mode_script=(ScriptMode)x; });

			config_set__script.add_line("DEFAULT MODE FOR FIRING");
			config_set__script.add_config_item(nameof(mode_fire__intra_group_default),() => mode_fire__intra_group_default,x => { mode_fire__intra_group_default=(FireMode)x; });
			config_set__script.add_config_item(nameof(mode_fire__inter_group_default),() => mode_fire__inter_group_default,x => { mode_fire__inter_group_default=(FireMode)x; });

			config_set__script.add_line("INDEXES OF CANNON AND GROUP SIZE");
			config_set__script.add_config_item(nameof(index__cannon_begin),() => index__cannon_begin,x => { index__cannon_begin=(int)x; });
			config_set__script.add_config_item(nameof(index__cannon_end),() => index__cannon_end,x => { index__cannon_end=(int)x; });
			config_set__script.add_config_item(nameof(number__cannons_in_group),() => number__cannons_in_group,x => { number__cannons_in_group=(int)x; });

			config_set__script.add_line("TAG OF THE GROUP NAME");
			config_set__script.add_config_item(nameof(tag__cannon_pistons_group),() => tag__cannon_pistons_group,x => { tag__cannon_pistons_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_releasers_group),() => tag__cannon_releasers_group,x => { tag__cannon_releasers_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_detachers_group),() => tag__cannon_detachers_group,x => { tag__cannon_detachers_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_shell_warheads_group),() => tag__cannon_shell_warheads_group,x => { tag__cannon_shell_warheads_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_detach_part_grid_locator),() => tag__cannon_detach_part_grid_locator,x => { tag__cannon_detach_part_grid_locator=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_shell_welders_group),() => tag__cannon_shell_welders_group,x => { tag__cannon_shell_welders_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_vertical_joints_group),() => tag__cannon_vertical_joints_group,x => { tag__cannon_vertical_joints_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_group_fire_indicators_group),() => tag__cannon_group_fire_indicators_group,x => { tag__cannon_group_fire_indicators_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__postfix_cannon_status_indicator),() => tag__postfix_cannon_status_indicator,x => { tag__postfix_cannon_status_indicator=(string)x; });
			config_set__script.add_config_item(nameof(tag__custom_interface_timer_0),() => tag__custom_interface_timer_0,x => { tag__custom_interface_timer_0=(string)x; });
			config_set__script.add_config_item(nameof(tag__custom_interface_timer_1),() => tag__custom_interface_timer_1,x => { tag__custom_interface_timer_1=(string)x; });
			config_set__script.add_config_item(nameof(tag__info_display_LCD_group),() => tag__info_display_LCD_group,x => { tag__info_display_LCD_group=(string)x; });
			config_set__script.add_config_item(nameof(tag__cannon_speed_limit_breaker),() => tag__cannon_speed_limit_breaker,x => { tag__cannon_speed_limit_breaker=(string)x; });

			config_set__script.add_line("FUNCTIONS SWITCH");
			config_set__script.add_config_item(nameof(flag__auto_activate_shell),() => flag__auto_activate_shell,x => { flag__auto_activate_shell=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_start_warhead_countdown),() => flag__auto_start_warhead_countdown,x => { flag__auto_start_warhead_countdown=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_toggle_welders_onoff),() => flag__auto_toggle_welders_onoff,x => { flag__auto_toggle_welders_onoff=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_toggle_vertical_joints_lock),() => flag__auto_toggle_vertical_joints_lock,x => { flag__auto_toggle_vertical_joints_lock=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_trigger_custom_interface_timer_0),() => flag__auto_trigger_custom_interface_timer_0,x => { flag__auto_trigger_custom_interface_timer_0=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_trigger_custom_interface_timer_1),() => flag__auto_trigger_custom_interface_timer_1,x => { flag__auto_trigger_custom_interface_timer_1=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_check),() => flag__auto_check,x => { flag__auto_check=(bool)x; });
			config_set__script.add_config_item(nameof(flag__auto_reload),() => flag__auto_reload,x => { flag__auto_reload=(bool)x; });
			config_set__script.add_config_item(nameof(flag__reload_once_after_script_initialization),() => flag__reload_once_after_script_initialization,x => { flag__reload_once_after_script_initialization=(bool)x; });
			config_set__script.add_config_item(nameof(flag__enable_two_stage_mode),() => flag__enable_two_stage_mode,x => { flag__enable_two_stage_mode=(bool)x; });
			config_set__script.add_config_item(nameof(flag__enable_shell_disconnection),() => flag__enable_shell_disconnection,x => { flag__enable_shell_disconnection=(bool)x; });

			config_set__script.add_line("EXECUTION CYCLE OF SOME FUNCTIONS");
			config_set__script.add_config_item(nameof(period__auto_check),() => period__auto_check,x => { period__auto_check=(int)x; });
			config_set__script.add_config_item(nameof(period__auto_fire),() => period__auto_fire,x => { period__auto_fire=(int)x; });
			config_set__script.add_config_item(nameof(period__update_output),() => period__update_output,x => { period__update_output=(int)x; });

			config_set__script.add_line("DELAY IN EXECUTING CANNON ACTION");
			config_set__script.add_config_item(nameof(delay_release),() => delay_release,x => { delay_release=(int)x; });
			config_set__script.add_config_item(nameof(delay__detach_begin),() => delay__detach_begin,x => { delay__detach_begin=(int)x; });
			config_set__script.add_config_item(nameof(delay_detach),() => delay_detach,x => { delay_detach=(int)x; });
			config_set__script.add_config_item(nameof(delay__detach_end),() => delay__detach_end,x => { delay__detach_end=(int)x; });
			config_set__script.add_config_item(nameof(delay__try_activate_shell),() => delay__try_activate_shell,x => { delay__try_activate_shell=(int)x; });
			config_set__script.add_config_item(nameof(delay__try_disconnect_shell),() => delay__try_disconnect_shell,x => { delay__try_disconnect_shell=(int)x; });
			config_set__script.add_config_item(nameof(delay__pistons_extend),() => delay__pistons_extend,x => { delay__pistons_extend=(int)x; });
			config_set__script.add_config_item(nameof(delay_attach),() => delay_attach,x => { delay_attach=(int)x; });
			config_set__script.add_config_item(nameof(delay__pistons_retract),() => delay__pistons_retract,x => { delay__pistons_retract=(int)x; });
			config_set__script.add_config_item(nameof(delay__custom_interface_timer_0),() => delay__custom_interface_timer_0,x => { delay__custom_interface_timer_0=(int)x; });
			config_set__script.add_config_item(nameof(delay__custom_interface_timer_1),() => delay__custom_interface_timer_1,x => { delay__custom_interface_timer_1=(int)x; });
			config_set__script.add_config_item(nameof(delay__done_loading),() => delay__done_loading,x => { delay__done_loading=(int)x; });

			config_set__script.add_config_item(nameof(delay__first_reload),() => delay__first_reload,x => { delay__first_reload=(int)x; });
			config_set__script.add_config_item(nameof(delay_pausing),() => delay_pausing,x => { delay_pausing=(int)x; });

			config_set__script.add_line("ABOUT SHELL ACTIVATION");
			config_set__script.add_config_item(nameof(distance__warhead_savety_lock),() => distance__warhead_savety_lock,x => { distance__warhead_savety_lock=(double)x; });
			config_set__script.add_config_item(nameof(number__warhead_countdown_seconds),() => number__warhead_countdown_seconds,x => { number__warhead_countdown_seconds=(double)x; });
			config_set__script.add_config_item(nameof(time__disconnecting_shell),() => time__disconnecting_shell,x => { time__disconnecting_shell=(int)x; });

			//config_set__t.add_line("ABOUT CHARGE-TWICE CANNON");

			config_set__t.add_config_item(nameof(tag__cannon_minor_pistons_group),() => tag__cannon_minor_pistons_group,x => { tag__cannon_minor_pistons_group=(string)x; });
			config_set__t.add_config_item(nameof(tag__cannon_fixators_group),() => tag__cannon_fixators_group,x => { tag__cannon_fixators_group=(string)x; });

			config_set__t.add_config_item(nameof(flag__synchronized_minor_pistons),() => flag__synchronized_minor_pistons,x => { flag__synchronized_minor_pistons=(bool)x; });

			config_set__t.add_config_item(nameof(delay__disable_fixators),() => delay__disable_fixators,x => { delay__disable_fixators=(int)x; });
			config_set__t.add_config_item(nameof(delay__minor_pistons_extend),() => delay__minor_pistons_extend,x => { delay__minor_pistons_extend=(int)x; });
			config_set__t.add_config_item(nameof(delay__enable_fixators),() => delay__enable_fixators,x => { delay__enable_fixators=(int)x; });
			config_set__t.add_config_item(nameof(delay__minor_pistons_retract),() => delay__minor_pistons_retract,x => { delay__minor_pistons_retract=(int)x; });

			config_script.add_config_set(config_set__script);

			config_script.parse_custom_data();
			if(flag__enable_two_stage_mode)
				config_script.add_config_set(config_set__t);
		}

		//检查脚本配置(配置合法返回null, 否则返回错误消息)
		string check_config()
		{
			string info = null;

			//检查必要编组的标签
			if(tag__cannon_pistons_group.Length==0
				||tag__cannon_releasers_group.Length==0
				||tag__cannon_detachers_group.Length==0)
			{
				info="<error_config> key tag of group name is empty";
				return info;
			}

			//检查火炮编号
			if(index__cannon_begin<0||index__cannon_end<0
				||index__cannon_begin>1000||index__cannon_end>1000
				||index__cannon_begin>index__cannon_end)
			{
				info="<error_config> illegal indexes of cannons";
				return info;
			}

			//检查组内火炮数量
			if(number__cannons_in_group<0||number__cannons_in_group>1000)
			{
				info=
					"<error_config> number of cannons in a group "+
					"cannot be less than 0 or more than 1000";
				return info;
			}

			if(!check_value(period__auto_check)
				||!check_value(period__auto_fire)
				||!check_value(period__update_output))
			{
				info="<error_config> period cannot be less than 1 or more than 1000";
				return info;
			}

			if(!check_value(delay_release)
				||!check_value(delay__detach_begin)
				||!check_value(delay_detach)
				||!check_value(delay__detach_end)
				||!check_value(delay__pistons_extend)
				||!check_value(delay_attach)
				||!check_value(delay__pistons_retract)
				||!check_value(delay__custom_interface_timer_0)
				||!check_value(delay__custom_interface_timer_1)
				||!check_value(delay__done_loading)
				)
			{
				info="<error_config> delay cannot be less than 1 or more than 1000";
				return info;
			}

			if(number__warhead_countdown_seconds<0.0||number__warhead_countdown_seconds>300.0)
			{
				info="<error_config> countdown cannot be less than 0.0 or more than 300.0";
				return info;
			}

			return null;
		}

		#endregion

		#region 类型定义

		//枚举 射击模式
		public enum FireMode
		{
			Salvo,//齐射
			Round,//轮射
			None,//空
		}

		//脚本运行模式
		public enum ScriptMode
		{
			Regular,//常规模式
			WeaponSync,//武器同步模式
		}

		//类 显示单元
		class DisplayUnit
		{
			//枚举 显示模式
			public enum DisplayMode
			{
				General,//一般信息显示
				SingleCannon,//单个火炮信息显示
				MultipleCannon,//多个火炮信息显示
				SingleGroup,//单个群组信息显示
				MultipleGroup,//多个群组信息显示
				None,//不显示内容
			}

			//LCD显示器
			//public IMyTextPanel lcd;
			//显示器
			public IMyTextSurface displayer;

			public DisplayMode mode_display = DisplayMode.General;

			//索引 开始 (列表容器中的索引)
			public int index_begin = -1;
			//索引 末尾 (列表容器中的索引)
			public int index_end = -1;
			//标记 是否图形化显示
			public bool flag_graphic = false;

			//构造函数
			public DisplayUnit(IMyTextSurface _display,int _index_begin = -1,int _index_end = -1,bool _flag_graphic = false)
			{
				this.displayer=_display;
				this.flag_graphic=_flag_graphic;
				this.index_begin=_index_begin;
				this.index_end=_index_end;
			}
		}

		/**************************************************
		* 类 PistonCannon
		* 一个 PistonCannon 类实例对应一根炮管
		**************************************************/
		public class PistonCannon
		{
			//枚举 火炮状态
			public enum CannonStatus
			{
				NotReady,//非就绪(活塞处于非蓄力状态等等)
				Ready,//就绪(可以射击)
				Loading,//装填中/运行中(射击后进入装填)
				BrokenDown,//故障(转子未能成功附着, 或者关键元件失去功能)
				Invalid,//不可用(火炮初始化时缺少关键元件)
				Pausing,//暂停中(活塞伸展后短暂暂停)
			}

			//枚举 火炮指令(火炮接下来需要执行的指令)
			public enum CannonCommand
			{
				Fire,//准备开火
				Reload,//重新装载
				None,//无指令
			}

			//分离模式
			public enum DetachMode
			{
				GatlinDestroy,//破坏分离(加特林)
				GrinderDestroy,//破坏分离(切割机)
				Normal,//常规分离(合并块)
				None,//哨兵
			}

			//固定模式 (二段式火炮)
			public enum FixMode
			{
				MergeBlock,//合并块
				MechanicalConnectionBlock,//机械连接块
				None,//哨兵
			}

			//火炮所属的组
			CannonGroup group;

			//编组 活塞元件
			IMyBlockGroup group_pistons;
			//编组 释放元件
			IMyBlockGroup group_releasers;
			//编组 分离元件
			IMyBlockGroup group_detachers;
			//编组 破速限元件
			IMyBlockGroup group__speed_limit_breaker;
			//编组 弹头元件
			IMyBlockGroup group_warheads;
			//编组 焊接器元件
			IMyBlockGroup group_welders;
			//编组 垂直关节元件
			IMyBlockGroup group__vertical_joints;
			//编组 次要活塞元件
			IMyBlockGroup group__minor_pistons;
			//编组 固定器元件
			IMyBlockGroup group__fixators;

			//列表 活塞元件
			List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();
			//列表 释放元件
			List<IMyMechanicalConnectionBlock> list_releasers = new List<IMyMechanicalConnectionBlock>();
			//列表 分离元件
			List<IMyFunctionalBlock> list_detachers = new List<IMyFunctionalBlock>();
			//列表 弹头元件
			List<IMyWarhead> list_warheads = new List<IMyWarhead>();
			//列表 焊接器元件
			List<IMyShipWelder> list_welders = new List<IMyShipWelder>();
			//列表 垂直关节元件
			List<IMyMotorStator> list__vertical_joints = new List<IMyMotorStator>();
			//列表 次要活塞元件
			List<IMyPistonBase> list__minor_pistons = new List<IMyPistonBase>();
			//列表 合并块元件
			//List<IMyShipMergeBlock> list_fixators = new List<IMyShipMergeBlock>();
			List<IMyTerminalBlock> list_fixators = new List<IMyTerminalBlock>();
			//列表 破速限元件(活塞)
			List<IMyPistonBase> list__speed_limmit_beakers = new List<IMyPistonBase>();

			//活塞 状态指示器
			IMyPistonBase piston__status_indicator;
			//活塞 次要状态指示器
			IMyPistonBase piston__minor_status_indicator;
			//转子 状态指示器 (取消这个设计)
			//IMyMechanicalConnectionBlock rotor__status_indicator;
			//定时器 用户定时器接口0
			IMyTimerBlock timer__custom_interface_0;
			//定时器 用户定时器接口1
			IMyTimerBlock timer__custom_interface_1;
			//定位器 火炮分离部件网格
			IMyTerminalBlock locator__cannon_detach_part_grid;
			//活塞 破速限元件
			IMyPistonBase piston__speed_limit_breaker;

			//网格 上一个炮弹的网格
			IMyCubeGrid grid__previous_shell;

			//索引 活塞炮的编号
			public int index_cannon { get; private set; } = -1;

			//编程块的this指针
			Program program;

			//标记 是否 自动激活炮弹
			bool flag__auto_activate_shell = true;
			//标记 检查炮弹分离状态
			bool flag__check_shell_detach_status = true;
			//标记 是否 自动切换焊接器开关状态
			bool flag__auto_toggle_welders_onoff = true;
			//标记 是否 自动切换垂直关节元件锁定状态
			bool flag__auto_toggle_vertical_joints_lock = true;
			//标记 是否 自动触发用户接口定时器0
			bool flag__auto_trigger_custom_interface_timer_0 = true;
			//标记 是否 自动触发用户接口定时器1
			bool flag__auto_trigger_custom_interface_timer_1 = true;
			//标记 是否 启用二段蓄力
			bool flag__enable_two_stage_mode = false;
			//标记 是否 启用主动断开炮弹连接功能
			bool flag__enable_shell_disconnection = false;
			//标记 是否 已经暂停过
			bool flag_paused = false;
			//标记 是否 全部释放器附着
			bool flag__all_releasers_attached = false;
			//标记 是否 全部固定器都被启用
			bool flag__all_fixators_enabled = false;

			//为了对抗K社的BUG而增加的两个变量
			//时刻 附着固定器
			int delay__detach_fixators = -1;
			//时刻 附着释放器
			int delay__detach_releasers = -1;
			//时刻 附着破限元件
			int delay__detach_speed_limit_breakers = -1;
			//时刻 重载时释放 (释放器)
			int delay__release_on_reload = 1;

			//次数 距离下一次 检查
			int times__before_next_check = 0;
			//次数 延迟(用于延迟重载和强制暂停)
			int times_delay = 0;

			//计数 状态计数
			public int count_status { get; private set; } = 1;

			//状态 火炮状态
			public CannonStatus status_cannon { get; private set; } = CannonStatus.NotReady;
			//指令 火炮操作指令
			public CannonCommand command_cannon { get; private set; } = CannonCommand.None;
			//模式 分离模式
			DetachMode mode_detach = DetachMode.None;
			//模式 固定模式(二段式火炮)
			FixMode mode_fix = FixMode.None;

			//字符串构建器 对象初始化时的信息
			StringBuilder string_builder__init_info = new StringBuilder();

			//构造函数(传递函数指针以及火炮索引)
			public PistonCannon(Program _program,int _index,CannonGroup _group)
			{
				//对成员进行赋值
				program=_program;
				index_cannon=_index;
				group=_group;


				//字符串 临时名称拼接
				string name_group;

				//清空字符串构建器
				string_builder__init_info.Clear();

				//初始化(获取控制过程中将使用的全部方块对象)

				/********************
				* 获取活塞元件
				********************/
				//拼接字符串, 得到编组名称
				name_group=this.program.tag__cannon_pistons_group+this.index_cannon;
				//获取活塞元件编组
				group_pistons=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group_pistons==null)
				{
					string_builder__init_info.Append($"<error> no group found with name \"{name_group}\"\n");
					status_cannon=CannonStatus.Invalid;
				}
				else
				{
					//添加到列表
					group_pistons.GetBlocksOfType<IMyPistonBase>(list_pistons);
					//检查
					if(list_pistons.Count==0)
					{
						string_builder__init_info.Append($"<error> no any piston found in group \"{name_group}\"\n");
						status_cannon=CannonStatus.Invalid;
					}
					else
					{
						foreach(var item in list_pistons)//逐个遍历活塞列表, 查找名称以指定规则结尾的活塞
							if(item.CustomName.EndsWith(program.tag__postfix_cannon_status_indicator))
								piston__status_indicator=item;//设置指示器
						if(piston__status_indicator==null)//如果 没有找到以指定后缀命名的活塞
							piston__status_indicator=list_pistons[0];//将指示器设置为列表中第一个活塞
					}
				}

				/********************
				* 获取释放元件
				********************/
				//拼接字符串, 得到编组名称
				name_group=this.program.tag__cannon_releasers_group+this.index_cannon;
				//获取活塞元件编组
				group_releasers=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group_releasers==null)
				{
					string_builder__init_info.Append($"<error> no group found with name \"{name_group}\"\n");
					status_cannon=CannonStatus.Invalid;
				}
				else
				{
					//添加到列表
					group_releasers.GetBlocksOfType<IMyMechanicalConnectionBlock>(list_releasers);
					//检查
					if(list_releasers.Count==0)
					{
						string_builder__init_info.Append($"<error> no any rotor or hinge found in group \"{name_group}\"\n");
						status_cannon=CannonStatus.Invalid;
					}
					//else
					//	rotor__status_indicator = list_releasers[0];//将第一个设为释放元件指示器
				}

				/********************
				* 获取分离元件
				********************/
				//拼接字符串, 得到编组名称
				name_group=this.program.tag__cannon_detachers_group+this.index_cannon;
				//获取活塞元件编组
				group_detachers=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group_detachers==null)
				{
					string_builder__init_info.Append($"<error> no group found with name \"{name_group}\"\n");
					status_cannon=CannonStatus.Invalid;
				}
				else
				{
					//添加到列表
					group_detachers.GetBlocksOfType<IMyFunctionalBlock>(list_detachers);
					//检查
					if(list_releasers.Count==0)
					{
						string_builder__init_info.Append($"<error> no gatlin or merge bLock found in group \"{name_group}\"\n");
						status_cannon=CannonStatus.Invalid;
					}
					else
					{
						var temp = list_detachers[0];

						//自动判断分离模式, 使用列表的第一个对象的类型进行判断
						if(temp is IMySmallGatlingGun)
							mode_detach=DetachMode.GatlinDestroy;
						else if(temp is IMyShipMergeBlock)
							mode_detach=DetachMode.Normal;
						else if(temp is IMyShipGrinder)
							mode_detach=DetachMode.GrinderDestroy;
						else
							//脚本不支持的分离模式
							status_cannon=CannonStatus.Invalid;
					}
				}

				/********************
				* 获取分离部件网格定位器
				********************/
				name_group=this.program.tag__cannon_detach_part_grid_locator+this.index_cannon;
				locator__cannon_detach_part_grid=program.GridTerminalSystem.GetBlockWithName(name_group);
				if(locator__cannon_detach_part_grid==null)
					string_builder__init_info.Append($"<warning> no block found with name \"{name_group}\" , ignored\n");

				/********************
				* 获取焊接器元件
				********************/
				//拼接字符串, 得到编组名称
				name_group=this.program.tag__cannon_shell_welders_group+this.index_cannon;
				//获取活塞元件编组
				group_welders=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group_welders==null)
				{
					string_builder__init_info.Append($"<warning> no group found with name \"{name_group}\" , ignored\n");
				}
				else
				{
					//添加到列表
					group_welders.GetBlocksOfType<IMyShipWelder>(list_welders);
					//检查
					if(list_welders.Count==0)
						string_builder__init_info.Append($"<warning> no any welder found in group \"{name_group}\"\n");
				}

				/********************
				* 获取垂直关节元件
				********************/
				//拼接字符串, 得到编组名称
				name_group=this.program.tag__cannon_vertical_joints_group+this.index_cannon;
				//获取活塞元件编组
				group__vertical_joints=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group__vertical_joints==null)
				{
					string_builder__init_info.Append($"<warning> no group found with name \"{name_group}\" , ignored\n");
				}
				else
				{
					//添加到列表
					group__vertical_joints.GetBlocksOfType<IMyMotorStator>(list__vertical_joints);
					//检查
					if(list__vertical_joints.Count==0)
						string_builder__init_info.Append($"<warning> no any rotor or hinge found in group \"{name_group}\"\n");
				}

				/********************
				* 获取用户自定义接口定时器
				********************/
				//接口0
				name_group=this.program.tag__custom_interface_timer_0+this.index_cannon;
				timer__custom_interface_0=program.GridTerminalSystem.GetBlockWithName(name_group) as IMyTimerBlock;
				if(timer__custom_interface_0==null)
					string_builder__init_info.Append($"<warning> no timer found with name \"{name_group}\" , ignored\n");

				//接口1
				name_group=this.program.tag__custom_interface_timer_1+this.index_cannon;
				timer__custom_interface_1=program.GridTerminalSystem.GetBlockWithName(name_group) as IMyTimerBlock;
				if(timer__custom_interface_1==null)
					string_builder__init_info.Append($"<warning> no timer found with name \"{name_group}\" , ignored\n");

				//启用炮弹分离功能
				if(program.flag__enable_shell_disconnection)
				{
					flag__enable_shell_disconnection=true;
					/********************
					* 获取破速限元件
					********************/
					name_group=this.program.tag__cannon_speed_limit_breaker+this.index_cannon;
					group__speed_limit_breaker=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
					if(group__speed_limit_breaker==null)
					{
						string_builder__init_info.Append($"<error> no group found with name \"{name_group}\", ignored, but the relevant funtion will be disabled\n");
						flag__enable_shell_disconnection=false;
					}
					else
					{
						group__speed_limit_breaker.GetBlocksOfType<IMyPistonBase>(list__speed_limmit_beakers);
						//检查
						if(list__speed_limmit_beakers.Count==0)
						{
							string_builder__init_info.Append($"<error> no pistons found in group \"{name_group}\", ignored, but the relevant funtion will be disabled\n");
							flag__enable_shell_disconnection=false;
						}
						else
							piston__speed_limit_breaker=list__speed_limmit_beakers.First();
						//piston__speed_limit_breaker=list__speed_limmit_beakers.Last();
					}
				}

				//启用二段蓄力模式
				if(program.flag__enable_two_stage_mode)
				{
					flag__enable_two_stage_mode=true;

					/********************
					* 获取次要活塞元件
					********************/
					//拼接字符串, 得到编组名称
					name_group=this.program.tag__cannon_minor_pistons_group+this.index_cannon;
					//获取活塞元件编组
					group__minor_pistons=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
					//检查
					if(group__minor_pistons==null)
					{
						string_builder__init_info.Append($"<warning> no group found with name \"{name_group}\" , ignored , but the charge-twice mode will be disabled\n");
						flag__enable_two_stage_mode=false;
					}
					else
					{
						//添加到列表
						group__minor_pistons.GetBlocksOfType<IMyPistonBase>(list__minor_pistons);
						//检查
						if(list_pistons.Count==0)
						{
							string_builder__init_info.Append($"<warning> no any piston found in group \"{name_group}\" , ignored , but the charge-twice mode will be disabled\n");
							flag__enable_two_stage_mode=false;
						}
						else
						{
							foreach(var item in list__minor_pistons)//逐个遍历活塞列表, 查找名称以指定规则结尾的活塞
								if(item.CustomName.EndsWith(program.tag__postfix_cannon_status_indicator))
									piston__minor_status_indicator=item;//设置指示器
							if(piston__minor_status_indicator==null)//如果 没有找到以指定后缀命名的活塞
								piston__minor_status_indicator=list__minor_pistons[0];//将指示器设置为列表中第一个活塞
						}
					}

					/********************
					* 获取合并块元件
					********************/
					//拼接字符串, 得到编组名称
					name_group=this.program.tag__cannon_fixators_group+this.index_cannon;
					//获取活塞元件编组
					group__fixators=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
					//检查
					if(group__fixators==null)
					{
						string_builder__init_info.Append($"<warning> no group found with name \"{name_group}\" , ignored , but the charge-twice mode will be disabled\n");
						flag__enable_two_stage_mode=false;
					}
					else
					{
						//添加到列表
						//group__fixators.GetBlocksOfType<IMyShipMergeBlock>(list_fixators);
						group__fixators.GetBlocks(list_fixators);//获取所有方块
																 //检查
						if(list_fixators.Count==0)
						{
							string_builder__init_info.Append($"<warning> no any block found in group \"{name_group}\" , ignored , but the charge-twice mode will be disabled\n");
							flag__enable_two_stage_mode=false;
						}
						else
						{
							var temp = list_fixators[0];

							//自动判断分离模式, 使用列表的第一个对象的类型进行判断
							if(temp is IMyShipMergeBlock)
								mode_fix=FixMode.MergeBlock;
							else if(temp is IMyMechanicalConnectionBlock)
								mode_fix=FixMode.MechanicalConnectionBlock;
							else
								//脚本不支持的固定模式
								flag__enable_two_stage_mode=false;
						}
					}
				}
				// 默认情况是装填完成时间-活塞收缩时间, 也就是活塞伸缩时长
				delay__release_on_reload=program.delay__done_loading-program.delay__pistons_retract;
				// 如果超过附着时间, 则改为附着时间
				if(delay__release_on_reload>=program.delay_attach)
					delay__release_on_reload=program.delay_attach-1;
				if(delay__release_on_reload<1)
					delay__release_on_reload=1;

				//大网格机械连接方块需要在附着前一帧释放, 否则无法附着
				delay__detach_releasers=program.delay_attach-1;
				delay__detach_speed_limit_breakers=program.delay__try_disconnect_shell+program.time__disconnecting_shell-1;
				delay__detach_fixators=program.delay__enable_fixators-1;

				//检查火炮是否可用
				if(status_cannon==CannonStatus.Invalid)
					return;

				//状态检查
				if(program.flag__reload_once_after_script_initialization||piston__status_indicator.Status!=PistonStatus.Retracted||!check_releasers_status())
				{
					status_cannon=CannonStatus.Pausing;//设置为暂停中
					command_cannon=CannonCommand.Reload;//设置延迟装载命令
					times_delay=program.delay__first_reload;//设置时刻时长
				}
				else
					status_cannon=CannonStatus.Ready;//设置为就绪状态

				//检查火炮内部功能开关

				//自动检查炮弹分离状态
				flag__check_shell_detach_status=
					locator__cannon_detach_part_grid!=null;
				//自动开关焊接器
				flag__auto_toggle_welders_onoff=
					program.flag__auto_toggle_welders_onoff
					&&group_welders!=null
					&&list_welders.Count!=0;
				//自动锁定垂直关节
				flag__auto_toggle_vertical_joints_lock=
					program.flag__auto_toggle_vertical_joints_lock
					&&group__vertical_joints!=null
					&&list__vertical_joints.Count!=0;
				//用户接口定时器0
				flag__auto_trigger_custom_interface_timer_0=
					program.flag__auto_trigger_custom_interface_timer_0
					&&timer__custom_interface_0!=null;
				//用户接口定时器1
				flag__auto_trigger_custom_interface_timer_1=
					program.flag__auto_trigger_custom_interface_timer_1
					&&timer__custom_interface_1!=null;
			}

			public void set_command(CannonCommand cmd)
			{
				this.command_cannon=cmd;
			}

			public PistonStatus get_piston_indicator_status()
			{
				if(piston__status_indicator!=null)
					return piston__status_indicator.Status;
				else
					return PistonStatus.Stopped;
			}

			//更新火炮
			public void update()
			{
				//检查火炮可用性
				if(status_cannon==CannonStatus.Invalid)
					return;

				//自动检查
				if(program.flag__auto_check&&times_delay==0)
				{
					if(times__before_next_check==0)
					{
						times__before_next_check=program.period__auto_check;
						self_status_check();
					}
					--times__before_next_check;
				}

				//根据当前状态执行操作
				switch(status_cannon)
				{
					case CannonStatus.NotReady://非就绪状态
						{
							//非就绪状态仅接受重载指令(和延迟重载)
							if(command_cannon==CannonCommand.Reload)//立即重载
							{
								//设置为运行状态
								status_cannon=CannonStatus.Loading;
								//运行
								run();
							}
						}
						break;
					case CannonStatus.Ready://就绪状态
						{
							//就绪状态可以接受所有指令
							if(command_cannon==CannonCommand.Fire)
							{
								//开火前检查一次
								if(!check_cannon_integrality()||!check_cannon_status())
								{
									status_cannon=CannonStatus.BrokenDown;
								}
								else
								{
									//更新上一次射击的火炮索引
									group.update_last_fire_index(this.index_cannon);

									//设置为运行状态
									status_cannon=CannonStatus.Loading;
									++program.count_fire;
									run();
								}
							}
							else if(command_cannon==CannonCommand.Reload)
							{
								//设置为运行状态
								status_cannon=CannonStatus.Loading;
								//运行
								run();
							}
						}
						break;
					case CannonStatus.Loading://运行状态
						{
							run();
						}
						break;
					case CannonStatus.Pausing://暂停中
						{
							if(times_delay<=0)
							{
								if(command_cannon==CannonCommand.Reload)
									status_cannon=CannonStatus.Loading;
								else
									status_cannon=CannonStatus.NotReady;
							}
							else
								--times_delay;
						}
						break;
					case CannonStatus.BrokenDown://故障
					case CannonStatus.Invalid://不可用
											  //处于故障状态和不可用状态的火炮不执行任何操作
						break;
				}

			}
			//自身状态检查
			public void self_status_check()
			{
				//检查完整性
				if(!check_cannon_integrality())
					//火炮不完整
					status_cannon=CannonStatus.BrokenDown;
				else
				{
					//火炮完整

					//检查状态检查之前的状态
					if(status_cannon==CannonStatus.BrokenDown)
						//之前不完整现在完整, 更新状态
						status_cannon=CannonStatus.Ready;

					if(status_cannon==CannonStatus.Ready)
					{
						//检查状态
						if(!check_cannon_status())
							status_cannon=CannonStatus.NotReady;
						else if(mode_detach!=DetachMode.Normal)
							//检查加特林或切割机是否处于关闭状态
							foreach(var item in list_detachers)
								if(item.Enabled)
									item.Enabled=false;
					}
					else if(status_cannon==CannonStatus.NotReady)
					{
						//检查状态
						if(check_cannon_status())
							status_cannon=CannonStatus.Ready;
						//尝试重载
						if(program.flag__auto_reload)
							command_cannon=CannonCommand.Reload;
					}
				}
			}

			//设置立刻执行检查
			public void check_once_immediately() => times__before_next_check=0;

			//内部动作执行函数
			private void run()
			{
				if(command_cannon==CannonCommand.None)
					return;
				//重载过程中在合适的时候释放
				if(command_cannon==CannonCommand.Reload&&count_status==delay__release_on_reload)
					release();//释放
				if(count_status==program.delay_release)
					if(command_cannon==CannonCommand.Fire)
						release();//释放
					else if(command_cannon==CannonCommand.Reload&&!flag_paused)
					{
						pistons_extend();//伸展
						times_delay=program.delay_pausing;//强制性暂停时间
						status_cannon=CannonStatus.Pausing;//设置暂停状态
						flag_paused=true;
						++this.count_status;
						return;//强制退出
					}

				//分离
				if(count_status==program.delay__detach_begin)
					if(command_cannon==CannonCommand.Fire)
						detach_begin();//开始分离
				if(count_status==program.delay_detach)
					if(command_cannon==CannonCommand.Fire)
						detach();//分离
				if(count_status==program.delay__detach_end)
					if(command_cannon==CannonCommand.Fire)
						detach_end();//结束分离

				//激活炮弹
				if(command_cannon==CannonCommand.Fire&&count_status==program.delay__try_activate_shell)
					try_activate_shell();
				//伸展
				if(command_cannon==CannonCommand.Fire&&count_status==program.delay__pistons_extend)
					pistons_extend();
				//解锁
				if(count_status==program.delay_attach)
					unlock_elevation_joints();
				//释放器预分离
				//if(count_status==delay__detach_releasers)
				//	releasers_detach();
				//附着
				if((!flag__all_releasers_attached&&count_status>program.delay_attach)||count_status==program.delay_attach)
					attach();

				if(count_status==program.delay__pistons_retract)
					pistons_retract();//收缩
				if(flag__auto_trigger_custom_interface_timer_0)
					if(count_status==program.delay__custom_interface_timer_0)
						custom_timer_interface_0();//第一个定时器接口
				if(flag__auto_trigger_custom_interface_timer_1)
					if(count_status==program.delay__custom_interface_timer_1)
						custom_timer_interface_1();//第二个定时器接口

				if(flag__enable_shell_disconnection)
				{
					if(count_status==program.delay__try_disconnect_shell&&command_cannon==CannonCommand.Fire)
						try_disconnect_shell_0();
					//破速限元件预分离
					//if(count_status==delay__detach_speed_limit_breakers&&command_cannon==CannonCommand.Fire)
					//	speed_limit_breakers_detach();
					if((!piston__speed_limit_breaker.IsAttached)
						&&(count_status>=program.delay__try_disconnect_shell+program.time__disconnecting_shell))
						try_disconnect_shell_1();
				}

				if(flag__enable_two_stage_mode)
				{
					if(count_status==program.delay__disable_fixators)
					{
						//禁用固定器
						disable_fixators();
						//若是重载命令, 则次要活塞伸展延迟到此步骤执行
						if(command_cannon==CannonCommand.Reload)
						{
							//伸展次要活塞
							minor_pistons_extend();

							// 强制暂停
							times_delay=program.delay_pausing;//强制性暂停时间
							status_cannon=CannonStatus.Pausing;//设置暂停状态
							flag_paused=true;
							++this.count_status;
							return;//强制退出
						}
					}
					if(command_cannon==CannonCommand.Fire&&count_status==program.delay__minor_pistons_extend)
						//伸展次要活塞
						minor_pistons_extend();
					//固定器预分离
					//if(count_status==delay__detach_fixators)
					//	fixators_detach();
					if(!flag__all_fixators_enabled&&count_status>=program.delay__enable_fixators)
						//启用固定器
						enable_fixators();
					if(count_status==program.delay__minor_pistons_retract)
						//收缩次要活塞
						minor_pistons_retract();
				}

				if(count_status==program.delay__done_loading)
				{
					done_command();//完成装填
					flag_paused=false;//重置
				}

				//状态计数+1
				++this.count_status;
			}

			//释放 同时会开启锁定垂直关节
			private void release()
			{
				//重新获取弹头
				if(program.flag__auto_activate_shell)
					update_warheads_list();

				//垂直转子锁定
				if(flag__auto_toggle_vertical_joints_lock)
					foreach(var item in list__vertical_joints)
						item.RotorLock=true;

				//释放
				foreach(var item in list_releasers)
					if(item.IsAttached)
						item.Detach();

				//打开焊接器
				if(flag__auto_toggle_welders_onoff)
				{
					foreach(var item in list_welders)
						item.Enabled=true;
				}

				//重置标记
				flag__all_releasers_attached=false;
			}

			private void detach_begin()
			{
				//根据不同的分离模式执行不同操作
				switch(mode_detach)
				{
					case DetachMode.GatlinDestroy://破坏式分离(加特林)
					case DetachMode.GrinderDestroy://破坏式分离(切割机)
						{
							//打开加特林
							//或
							//打开切割机(开始切割)
							foreach(var item in list_detachers)
								item.Enabled=true;
						}
						break;
					case DetachMode.Normal://常规分离(合并块)
						break;
				}
			}

			private void detach()
			{
				//根据不同的分离模式执行不同操作
				switch(mode_detach)
				{
					case DetachMode.GatlinDestroy://破坏式分离(加特林)
						{
							//射击一次
							foreach(var item in list_detachers)
								item.ApplyAction("ShootOnce");
						}
						break;
					case DetachMode.GrinderDestroy://破坏式分离(切割机)
					case DetachMode.Normal://常规分离(合并块)
						{
							//关闭切割机或合并块
							foreach(var item in list_detachers)
								item.Enabled=false;
						}
						break;
				}
			}

			//分离结束
			private void detach_end()
			{
				//根据不同的分离模式执行不同操作
				switch(mode_detach)
				{
					case DetachMode.GatlinDestroy://破坏式分离(加特林)
						{
							//关闭加特林
							foreach(var item in list_detachers)
								item.Enabled=false;
						}
						break;
					case DetachMode.GrinderDestroy://破坏式分离(切割机)
						break;
					case DetachMode.Normal://常规分离(合并块)
						{
							//开启合并块
							foreach(var item in list_detachers)
								item.Enabled=true;
						}
						break;
				}
			}

			//尝试激活炮弹
			private void try_activate_shell()
			{
				//激活弹头
				if(flag__auto_activate_shell)
				{
					if(flag__check_shell_detach_status)
					{
						//获取网格
						IMyCubeGrid grid_locator = locator__cannon_detach_part_grid.CubeGrid;
						//炮弹所在网格
						IMyCubeGrid grid_shell = null;
						if(list_warheads.Count>0)
							grid_shell=list_warheads[0].CubeGrid;
						else
							return;
						//直线
						LineD line = new LineD(locator__cannon_detach_part_grid.GetPosition(),list_warheads[0].GetPosition());
						//检查
						if(grid_locator!=grid_shell&&line.Length>program.distance__warhead_savety_lock)
						{
							if(program.flag__auto_start_warhead_countdown)
								foreach(var item in list_warheads)
								{
									item.DetonationTime=(float)program.number__warhead_countdown_seconds;
									item.StartCountdown(); item.IsArmed=true;
								}
							else
								foreach(var item in list_warheads)
									item.IsArmed=true;
						}
					}
					else
					{
						//直接激活
						if(program.flag__auto_start_warhead_countdown)
							foreach(var item in list_warheads)
							{
								item.DetonationTime=(float)program.number__warhead_countdown_seconds;
								item.StartCountdown(); item.IsArmed=true;
							}
						else
							foreach(var item in list_warheads)
								item.IsArmed=true;
					}

				}
			}

			//活塞伸展
			private void pistons_extend()
			{
				//检查状态 活塞不处于已伸展也未正在伸展
				if(piston__status_indicator.Status!=PistonStatus.Extended&&piston__status_indicator.Status!=PistonStatus.Extending)
					foreach(var item in list_pistons)
						item.Extend();
			}

			//释放器预分离
			private void releasers_detach()
			{
				foreach(var item in list_releasers)
					item.Detach();//由于傻逼K社的智障BUG, 必须加上这一行
			}

			//附着 同时会解锁垂直关节
			private void attach()
			{
				//附着
				foreach(var item in list_releasers)
					if(!item.IsAttached)//检查是否已经附着
					{
						item.Detach();//由于傻逼K社的智障BUG, 必须加上这一行
						item.Attach();
					}
				flag__all_releasers_attached=check_releasers_status();//更新一次
			}

			//解锁垂直关节
			private void unlock_elevation_joints()
			{
				if(command_cannon==CannonCommand.Fire)
					if(flag__auto_toggle_vertical_joints_lock)//垂直转子解锁
						foreach(var item in list__vertical_joints)
							item.RotorLock=false;
			}

			//活塞收缩
			private void pistons_retract()
			{
				//检查状态 活塞不处于已收缩也未正在收缩
				if(piston__status_indicator.Status!=PistonStatus.Retracted
					&&piston__status_indicator.Status!=PistonStatus.Retracting)
				{
					foreach(var item in list_pistons)
						item.Retract();
				}
			}

			private void custom_timer_interface_0()
			{
				//触发用户定时器0
				timer__custom_interface_0.Trigger();
			}

			private void custom_timer_interface_1()
			{
				//触发用户定时器1
				timer__custom_interface_1.Trigger();
			}

			private void try_disconnect_shell_0() => piston__speed_limit_breaker.Detach();

			//破速限元件预分离
			private void speed_limit_breakers_detach()
			{
				piston__speed_limit_breaker.Detach();
			}

			private void try_disconnect_shell_1()
			{
				piston__speed_limit_breaker.Detach();//由于傻逼K社的智障BUG, 必须加上这一行
				piston__speed_limit_breaker.Attach();
			}

			//禁用固定器
			private void disable_fixators()
			{
				switch(mode_fix)
				{
					case FixMode.MergeBlock:
						{
							foreach(var item in list_fixators)
								(item as IMyFunctionalBlock).Enabled=false;//关闭
						}
						break;
					case FixMode.MechanicalConnectionBlock:
						{
							foreach(var item in list_fixators)
								(item as IMyMechanicalConnectionBlock).Detach();//分离
						}
						break;
				}
				flag__all_fixators_enabled=false;//重置标记
			}

			//次要活塞伸展(伸展按同步的次要活塞伸展定义)
			private void minor_pistons_extend()
			{
				if(program.flag__synchronized_minor_pistons)
				{
					if(piston__minor_status_indicator.Status!=PistonStatus.Extended&&piston__minor_status_indicator.Status!=PistonStatus.Extending)
						foreach(var item in list__minor_pistons)
							item.Extend();
				}
				else
				{
					if(piston__minor_status_indicator.Status!=PistonStatus.Retracted&&piston__minor_status_indicator.Status!=PistonStatus.Retracting)
						foreach(var item in list__minor_pistons)
							item.Retract();
				}
			}

			//固定器预分离
			private void fixators_detach()
			{
				if(mode_fix==FixMode.MechanicalConnectionBlock)
					foreach(var item in list_fixators)
						(item as IMyMechanicalConnectionBlock).Detach();
			}

			//启用固定器
			private void enable_fixators()
			{
				switch(mode_fix)
				{
					case FixMode.MergeBlock:
						{
							foreach(var item in list_fixators)
								(item as IMyFunctionalBlock).Enabled=true;//关闭
						}
						break;
					case FixMode.MechanicalConnectionBlock:
						{
							foreach(var item in list_fixators)
							{
								(item as IMyMechanicalConnectionBlock).Detach();//由于傻逼K社的智障BUG, 必须加上这一行
								(item as IMyMechanicalConnectionBlock).Attach();//附着
							}
						}
						break;
				}
				flag__all_fixators_enabled=check_fixators_status();//检查状态
			}

			//次要活塞收缩(收缩按同步的次要活塞收缩定义)
			private void minor_pistons_retract()
			{
				if(program.flag__synchronized_minor_pistons)
				{
					if(piston__minor_status_indicator.Status!=PistonStatus.Retracted&&piston__minor_status_indicator.Status!=PistonStatus.Retracting)
						foreach(var item in list__minor_pistons)
							item.Retract();
				}
				else
				{
					if(piston__minor_status_indicator.Status!=PistonStatus.Extended&&piston__minor_status_indicator.Status!=PistonStatus.Extending)
						foreach(var item in list__minor_pistons)
							item.Extend();
				}
			}

			//结束命令 同时会关闭焊接器
			private void done_command()
			{
				//关闭焊接器
				if(flag__auto_toggle_welders_onoff)
					foreach(var item in list_welders)
						item.Enabled=false;
				//重置为就绪态
				this.status_cannon=CannonStatus.Ready;
				//重置命令
				this.command_cannon=CannonCommand.None;
				//重置状态计数器(之后会自动+1)
				count_status=0;
			}

			//更新弹头列表, 弹头在焊接之后需要重新获取
			private void update_warheads_list()
			{
				//拼接字符串, 得到编组名称
				string name_group = this.program.tag__cannon_shell_warheads_group+this.index_cannon;
				//清空旧列表
				list_warheads.Clear();
				//获取弹头元件编组
				group_warheads=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group_warheads!=null)
					//添加到列表
					group_warheads.GetBlocksOfType<IMyWarhead>(list_warheads);

				//将上一发炮弹的弹头从列表中剔除
				for(var i = 0;i<list_warheads.Count;++i)
					if(list_warheads[i].CubeGrid==grid__previous_shell)
						list_warheads.RemoveAt(i--);
				//更新网格记录
				if(list_warheads.Count>0)
					grid__previous_shell=list_warheads[0].CubeGrid;
				else
					grid__previous_shell=null;

				this.flag__auto_activate_shell=(group_warheads!=null)&&(list_warheads.Count!=0);
			}

			//检查释放器状态
			private bool check_releasers_status()
			{
				foreach(var item in list_releasers)
					if(!item.IsAttached)//发现没有成功附着的则返回false
						return false;
				return true;//全部附着返回true
			}

			//检查固定器状态
			private bool check_fixators_status()
			{
				switch(mode_fix)
				{
					case FixMode.MergeBlock:
						{
							var set = new HashSet<IMyCubeGrid>();
							foreach(var item in list_fixators)
							{
								set.Add(item.CubeGrid);
								if(!(item as IMyShipMergeBlock).IsConnected)//发现没有成功连接的则返回false
									return false;
							}
							return set.Count==1;//只有一个相同网格返回true
						}
					case FixMode.MechanicalConnectionBlock:
						{
							foreach(var item in list_fixators)
							{
								if(!(item as IMyMechanicalConnectionBlock).IsAttached)
									return false;
							}
							return list_fixators.Count!=0;
						}
					default:
						return false;
				}

			}

			private bool check_detachers_status()
			{
				switch(mode_detach)
				{
					//加特林无需检查
					case DetachMode.GatlinDestroy:
						return true;
					//切割机需要保证处于关闭状态
					case DetachMode.GrinderDestroy:
						foreach(var item in list_detachers)
							if(item.Enabled)
								return false;
						break;
					//合并块需要保证处于连接状态
					case DetachMode.Normal:
						foreach(var item in list_detachers)
							if(!(item as IMyShipMergeBlock).IsConnected)
								return false;
						break;
				}
				return true;
			}

			//检查破限元件状态
			private bool check_speed_limit_breakers_status()
			{
				foreach(var item in list__speed_limmit_beakers)
					if(!item.IsAttached)
						return false;
				return true;
			}

			//检查火炮在在物理上的状态
			private bool check_cannon_status()
			{
				//检查活塞状态指示器
				if(piston__status_indicator.Status!=PistonStatus.Retracted&&piston__status_indicator.Status!=PistonStatus.Retracting)
					return false;
				//检查释放器状态
				else if(!check_releasers_status())
					return false;
				//检查固定器状态
				else if(flag__enable_two_stage_mode&&!check_fixators_status())
					return false;
				//检查破速限元件状态
				else if(flag__enable_shell_disconnection&&!check_speed_limit_breakers_status())
					return false;
				//检查分离器状态
				else if(!check_detachers_status())
					return false;
				return true;
			}

			//检查火炮完整性
			private bool check_cannon_integrality()
			{
				//结果
				bool flag_res = true;

				//检查活塞元件完整性
				foreach(var item in list_pistons)
					if(!item.IsWorking||item.GetPosition().IsZero())
					{
						flag_res=false;
						break;
					}

				//检查释放元件完整性
				foreach(var item in list_releasers)
					if(!item.IsFunctional||item.GetPosition().IsZero())
					{
						flag_res=false;
						break;
					}

				//检查分离器完整性
				foreach(var item in list_detachers)
					if(!item.IsFunctional||item.GetPosition().IsZero())
					{
						flag_res=false;
						break;
					}

				return flag_res;
			}

			//获取火炮显示信息
			public string get_cannon_info()
			{
				return "<cannon> -------------------- No."+this.index_cannon
					+"\n<count_status> "+this.count_status
					+"\n<mode_detach> "+this.mode_detach
					+"\n<status> "+status_cannon.ToString()
					+"\n<command> "+command_cannon.ToString()
					+"\n<rotor_attached> "+this.check_releasers_status()
					+"\n<status_pistons> "+this.get_piston_indicator_status()
					+"\n<count_pistons> "+this.list_pistons.Count
					+"\n<delay_ROR> "+this.delay__release_on_reload
					+"\n<flag_AAS> "+this.flag__auto_activate_shell
					+"\n<flag_ATWOO> "+this.flag__auto_toggle_welders_onoff
					+"\n<flag_ATVJL> "+this.flag__auto_toggle_vertical_joints_lock
					+"\n<flag_ATCIT0> "+this.flag__auto_trigger_custom_interface_timer_0
					+"\n<flag_ATCIT1> "+this.flag__auto_trigger_custom_interface_timer_1
					+"\n<flag_ESD> "+this.flag__enable_shell_disconnection
					+"\n<flag_ECTM> "+this.flag__enable_two_stage_mode
					+"\n\n <init_info> \n"+string_builder__init_info.ToString();
			}

			//获取火炮LCD显示信息
			public string get_cannon_LCD_info()
			{
				return "--------------------<cannon>--------------------"
					+"\n<cannon> No."+this.index_cannon
					+"\n<count_status> "+this.count_status
					+"\n<status> "+status_cannon.ToString()
					+"\n<command> "+command_cannon.ToString()
					+"\n<rotor_attached> "+this.check_releasers_status()
					+"\n<status_pistons> "+this.get_piston_indicator_status()
					+"\n<count_pistons> "+this.list_pistons.Count
					+"\n<flag_AAS> "+this.flag__auto_activate_shell
					+"\n<flag_ATWOO> "+this.flag__auto_toggle_welders_onoff
					+"\n<flag_ATVJL> "+this.flag__auto_toggle_vertical_joints_lock
					+"\n<flag_ATCIT0> "+this.flag__auto_trigger_custom_interface_timer_0
					+"\n<flag_ATCIT1> "+this.flag__auto_trigger_custom_interface_timer_1
					+"\n<flag_ESD> "+this.flag__enable_shell_disconnection
					+"\n<flag_ECTM> "+this.flag__enable_two_stage_mode;
			}

			//获取火炮简易信息
			public string get_cannon_simple_info()
			{
				return
					"<cannon> No."+this.index_cannon
					+" "+this.status_cannon.ToString()
					+" "+this.command_cannon.ToString()
					+" "+get_string_progress_bar();
			}

			//获取字符串进度条
			private StringBuilder get_string_progress_bar()
			{
				int count = (int)(((count_status+program.delay__done_loading-2)%program.delay__done_loading)/(double)program.delay__done_loading*11);
				StringBuilder builder_str = new StringBuilder("          ");
				for(int i = 0;i<count;++i)
					builder_str[i]='#';
				return builder_str;
			}
		}

		/**************************************************
		* 类 CannonGroup
		* 火炮对象的编组管理对象
		**************************************************/
		public class CannonGroup
		{
			//编组 射击指示器
			IMyBlockGroup group__fire_indicators;

			//列表 火炮
			List<PistonCannon> list_cannons = new List<PistonCannon>();

			//列表 射击指示器元件
			List<IMyUserControllableGun> list__fire_indicators = new List<IMyUserControllableGun>();

			//编组内部射击模式
			FireMode mode_fire = FireMode.Round;

			int index_group = -1;

			//索引 上一次射击(的火炮)(List容器中的索引)
			int index__cannon_last_fire = -1;
			//索引 火炮起始(本编组管理的火炮起始编号, 含)
			int index__cannon_begin = -1;
			//索引 火炮末尾(本编组管理的火炮末尾编号, 含)
			int index__cannon_end = -1;
			//索引 火炮起始(List容器中的索引, 含)
			int index_begin = -1;
			//索引 火炮末尾(List容器中的索引, 含)
			int index_end = -1;

			//标记 是否 射击指示器处于激活状态
			bool flag__fire_indicators_activated = false;

			//编程块的this指针
			Program program;

			StringBuilder string_builder__init_info = new StringBuilder();

			//构造函数
			public CannonGroup(Program _program,int _index_group)
			{
				program=_program;
				index_group=_index_group;

				/********************
				* 获取射击指示器元件
				********************/
				//拼接字符串, 得到编组名称
				string name_group = this.program.tag__cannon_group_fire_indicators_group+this.index_group;
				//获取射击指示器元件编组
				group__fire_indicators=program.GridTerminalSystem.GetBlockGroupWithName(name_group);
				//检查
				if(group__fire_indicators==null)
				{
					string_builder__init_info.Append($"<warning> no group found with name \"{name_group}\" , ignored\n");
				}
				else
				{
					//添加到列表
					group__fire_indicators.GetBlocksOfType<IMyUserControllableGun>(list__fire_indicators);
					//检查
					if(list__fire_indicators.Count==0)
					{
						string_builder__init_info.Append($"<warning> no any weapon found in group \"{name_group}\"\n");
					}
				}

				mode_fire=program.mode_fire__intra_group;
				return;
			}

			public void set_group_fire_mode(FireMode mode)
			{
				this.mode_fire=mode;
			}

			//添加火炮对象到编组
			public void add_cannon(PistonCannon cannon)
			{
				//设置起始索引
				if(list_cannons.Count==0)
				{
					index__cannon_begin=cannon.index_cannon;
					index_begin=index__cannon_begin-program.index__cannon_begin;
				}
				//添加到列表
				list_cannons.Add(cannon);
				index__cannon_last_fire=list_cannons.Count-1;
				//更新末尾索引
				index__cannon_end=list_cannons[list_cannons.Count-1].index_cannon;
				index_end=index__cannon_end-program.index__cannon_begin;
			}

			public int get___index__cannon_begin()
			{
				return index__cannon_begin;
			}

			public int get___index__cannon_end()
			{
				return index__cannon_end;
			}

			public int get__index_begin()
			{
				return index_begin;
			}

			public int get__index_end()
			{
				return index_end;
			}

			//编组射击
			//返回有几门火炮进行射击
			public int fire(FireMode mode = FireMode.None)
			{
				if(program.mode_script==ScriptMode.WeaponSync&&!flag__fire_indicators_activated)
					return 0;

				int count = 0;
				var mode_temp = mode==FireMode.None ? mode_fire : mode;
				switch(mode_temp)
				{
					case FireMode.Salvo://齐射
						count=fire_salvo();
						break;
					case FireMode.Round://轮射
						count=fire_round();
						break;
				}
				//更新上一次射击的编组
				if(count!=0)
					program.index__group_last_fire=index_group;
				return count;
			}

			//齐射 
			//返回有几门火炮进行射击
			private int fire_salvo()
			{
				int count = 0;

				//逐个遍历寻找所有处于 Ready 状态的火炮
				foreach(var item in list_cannons)
				{
					//检查状态
					if(item.status_cannon==PistonCannon.CannonStatus.Ready)
					{
						//设置射击命令
						item.set_command(PistonCannon.CannonCommand.Fire);
						++count;
					}
				}
				return count;
			}

			//轮射
			//返回有几门火炮进行射击
			private int fire_round()
			{
				int i = -1;

				//从上一次开炮的位置往后查找到末尾
				int index__last_next = index__cannon_last_fire+1;

				for(i=index__last_next;i<list_cannons.Count;++i)
				{
					//检查状态
					if(list_cannons[i].status_cannon==PistonCannon.CannonStatus.Ready)
					{
						//设置射击命令
						list_cannons[i].set_command(PistonCannon.CannonCommand.Fire);
						return 1;
					}
				}

				//仍然没有找到
				if(i==list_cannons.Count)
				{
					//从开头重新查找到上次开炮的位置
					for(i=0;i<index__last_next;++i)
					{
						//检查状态
						if(list_cannons[i].status_cannon==PistonCannon.CannonStatus.Ready)
						{
							//设置射击命令
							list_cannons[i].set_command(PistonCannon.CannonCommand.Fire);
							return 1;
						}
					}
				}
				return 0;
			}

			//检车并更新武器同步状态
			public bool check_and_update_weapon_synchronization_status()
			{
				flag__fire_indicators_activated=false;
				foreach(var item in list__fire_indicators)
					if(item.IsShooting)
					{
						flag__fire_indicators_activated=true;
						break;
					}
				return flag__fire_indicators_activated;
			}

			//更新上一次射击火炮索引
			public void update_last_fire_index(int index)
			{
				this.index__cannon_last_fire=index-index__cannon_begin;
			}

			//返回就绪的火炮数量
			private int count_ready_cannon()
			{
				int count = 0;
				foreach(var item in list_cannons)
					if(item.status_cannon==PistonCannon.CannonStatus.Ready)
						++count;
				return count;
			}

			//获取火炮信息
			public string get_group_info()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(
					"\n<group> ------------------------------ No."+this.index_group
					+"\n<mode_fire> "+this.mode_fire
					+"\n\n");
				foreach(var item in list_cannons)
					sb.Append(item.get_cannon_info()+"\n");
				sb.Append(string_builder__init_info);
				return sb.ToString();
			}

			public string get_group_LCD_info()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(
					"--------------------<group>--------------------"
					+"\n<group> No."+this.index_group
					+"\n<mode_fire> "+this.mode_fire
					+"\n----------<cannons>----------\n");
				foreach(var item in list_cannons)
					sb.Append(item.get_cannon_simple_info()+"\n");
				return sb.ToString();
			}

			internal void toggle_group_fire_mode()
			{
				if(mode_fire==FireMode.Round)
					mode_fire=FireMode.Salvo;
				else
					mode_fire=FireMode.Round;
			}
		}

		#endregion

		#region 配置通用

		/***************************************************************************************************
		* 类 自定义数据配置
		* 自定义数据配置(下简称CD配置)使用目标方块的自定义数据来进行脚本配置
		* 支持动态配置, 标题等, 功能强大
		***************************************************************************************************/

		//管理对象
		public class CustomDataConfig
		{
			//分割线 标题
			public static string separator_title = "##########";
			//分割线 副标题
			public static string separator_subtitle = "-----";
			//映射表 配置项集合
			Dictionary<string,CustomDataConfigSet> dict__config_sets = new Dictionary<string,CustomDataConfigSet>();
			//映射表 字符串内容
			Dictionary<string,List<string>> dict__str_contents = new Dictionary<string,List<string>>();
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
				this.block_target=block_target;
			}

			//初始化配置
			public void init_config()
			{
				parse_custom_data();//解析自定义数据
				if(!flag__config_error)//检查CD配置是否存在错误
					write_to_block_custom_data();//写入自定义数据
			}

			//添加配置集
			public bool add_config_set(CustomDataConfigSet set)
			{
				if(dict__config_sets.ContainsKey(set.title__config_set))
					return false;
				dict__config_sets.Add(set.title__config_set,set);
				return true;
			}

			//解析CD(拆分)
			public void parse_custom_data()
			{
				//以换行符拆分
				string[] array_lines = block_target.CustomData.Split('\n');
				string pattern = $"{separator_title} (.+) {separator_title}";//正则表达式
				var regex = new System.Text.RegularExpressions.Regex(pattern);
				string title_crt = "";
				foreach(var line in array_lines)
				{
					var match = regex.Match(line);//正则匹配
					if(line.Length==0) continue;
					else if(match.Success)
						dict__str_contents[title_crt=match.Groups[1].ToString()]=new List<string>();
					else if(dict__str_contents.ContainsKey(title_crt))
						dict__str_contents[title_crt].Add(line);
					else
					{
						string_builder__error_info.Append($"<error> illegal CD config data: \n{line}\n");
						flag__config_error=true; break;
					}
				}
				foreach(var pair in dict__str_contents)
				{
					if(dict__config_sets.ContainsKey(pair.Key))
						if(!dict__config_sets[pair.Key].parse_string_data(pair.Value))
						{
							string_builder__error_info.Append($"<error> illegal config item in config set: [{pair.Key}]\n{dict__config_sets[pair.Key].string_builder__error_info}\n");
							flag__config_error=true; break;
						}
				}
			}

			//写入方块CD
			public void write_to_block_custom_data()
			{
				foreach(var item in dict__config_sets)
				{
					item.Value.generate_string_data();
					string_builder__data.Append(item.Value.string_builder__data);
				}
				block_target.CustomData=string_builder__data.ToString();
			}
		}
		//CD配置集合
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
				public ConfigItemReference(Func<object> _getter,Action<object> _setter)
				{
					get=_getter; set=_setter;
				}
			}

			//配置集标题
			public string title__config_set { get; private set; }

			//字典 配置项字典
			Dictionary<string,ConfigItemReference> dict__config_items = new Dictionary<string,ConfigItemReference>();

			//字符串构建器 数据
			public StringBuilder string_builder__data { get; private set; } = new StringBuilder();
			//字符串构建器 错误信息
			public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();

			//标记 配置中发现错误(存在错误时不会覆盖写入)
			public bool flag__config_error { get; private set; } = false;

			//构造函数
			public CustomDataConfigSet(string title_config = "SCRIPT CONFIGURATION")
			{
				this.title__config_set=title_config;
			}

			//添加配置项
			public bool add_config_item(string name_config_item,Func<object> getter,Action<object> setter)
			{
				if(dict__config_items.ContainsKey(name_config_item))
					return false;
				dict__config_items.Add(name_config_item,new ConfigItemReference(getter,setter));
				return true;
			}

			//添加分割线
			public bool add_line(string str_title)
			{
				//检查是否已经包含此配置项目
				if(dict__config_items.ContainsKey(str_title))
					return false;
				//添加到字典
				dict__config_items.Add(str_title,null);
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

				foreach(var line in content)
				{
					++count;
					if(line.Length==0)
						continue;//跳过空行
					if(line.StartsWith(CustomDataConfig.separator_subtitle))
						continue;
					//以等号拆分
					string[] pair = line.Split('=');
					//检查
					if(pair.Length!=2)
					{
						string_builder__error_info.Append($"<error> \"{line}\"(at line {count}) is not a legal config item");
						flag__config_error=true;
						continue;//跳过配置错误的行
					}

					//去除多余空格
					string name__config_item = pair[0].Trim();
					string str_value__config_item = pair[1].Trim();

					ConfigItemReference reference;
					//尝试获取
					if(!dict__config_items.TryGetValue(name__config_item,out reference))
						continue;//不包含值, 跳过

					//包含值, 需要解析并更新
					var value = reference.get();//获取值
					if(parse_string(str_value__config_item,ref value))
						//成功解析字符串, 更新数值
						dict__config_items[name__config_item].set(value);
					else
					{
						//解析失败
						string_builder__error_info.Append($"<error> \"{str_value__config_item}\"(at line {count}) is not a legal config value");
						flag__config_error=true;
					}
				}
				return !flag__config_error;
			}

			//生成字符串数据
			public void generate_string_data()
			{
				int count = 0;

				string_builder__data.Clear();
				string_builder__data.Append($"{CustomDataConfig.separator_title} {title__config_set} {CustomDataConfig.separator_title}\n");
				foreach(var pair in dict__config_items)
				{
					if(pair.Value!=null)
						string_builder__data.Append($"{pair.Key} = {pair.Value.get()}\n");
					else
					{
						if(count!=0) string_builder__data.Append("\n");
						string_builder__data.Append($"{CustomDataConfig.separator_subtitle} {pair.Key} {CustomDataConfig.separator_subtitle}\n");
					}
					++count;
				}
				string_builder__data.Append("\n\n");
			}

			//解析字符串值
			private bool parse_string(string str,ref object v)
			{
				if(v is bool)
				{
					bool value_parsed;
					if(bool.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is float)
				{
					float value_parsed;
					if(float.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is double)
				{
					double value_parsed;
					if(double.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is int)
				{
					int value_parsed;
					if(int.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is Vector3D)
				{
					Vector3D value_parsed;
					if(Vector3D.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is string)
				{
					v=str;
					return true;
				}
				else if(v is ScriptMode)
				{
					ScriptMode value_parsed;
					if(ScriptMode.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				else if(v is FireMode)
				{
					FireMode value_parsed;
					if(FireMode.TryParse(str,out value_parsed))
					{
						v=value_parsed;
						return true;
					}
				}
				return false;
			}
		}

		#endregion

#if DEVELOP
	}
}
#endif
