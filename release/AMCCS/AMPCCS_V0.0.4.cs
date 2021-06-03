/********************************************************************************************************************************************************************************************************
*                                                                 _                   
*   /\   _|     _. ._   _  _   _|   |\/|     | _|_ o ._  |  _    |_) o  _ _|_  _  ._  
*  /--\ (_| \/ (_| | | (_ (/_ (_|   |  | |_| |  |_ | |_) | (/_   |   | _>  |_ (_) | | 
*                                                    |                                
*   _                        _                       __                               
*  /   _. ._  ._   _  ._    /   _  ._ _|_ ._ _  |   (_      _ _|_  _  ._ _            
*  \_ (_| | | | | (_) | |   \_ (_) | | |_ | (_) |   __) \/ _>  |_ (/_ | | |           
*                                                       /                             
* 
* ### Advanced Multiple Piston Cannon Control System (by SiriusZ-BOTTLE) ###
* ### 高级多联装活塞火炮控制系统 (by SiriusZ-瓶子) ###
* ### Version 0.0.4 ###
* 
* 
* ============================= 傻瓜式手册 =============================
* (如果你只想使用此脚本的核心功能, 并且懒得看复杂的说明文档, 请看这里, 其它都可不看)
* 
* [0] 在网格上面放置一个编程块, 把脚本的所有内容粘贴进去, 点确定
* [1] 把你的第一根火炮的蓄力活塞编组为 "Pistons_cannon_#0"(勿带双引号, 下同), 
*     第二根的活塞编组为 "Pistons_cannon_#1", 以此类推
* [2] 把你的第一根火炮的释放转子编组为 "Releasers_cannon_#0" ...以此类推...
* [3] 把你的第一根火炮的分离加特林编组为 "Detachers_cannon_#0" ...以此类推...
* [4] 如果你有三根炮, 就在选项里面把 index__cannon_end 设置成 2, 两根就设为 1, 
*     默认是从 0 开始计数的 ...以此类推...
* [5] 设置完成之后需要在编程块中重置脚本一次, 最后在工具栏中添加该编程块, 
*     找到并点击 "运行" 条目, SE会提示你输入运行参数, 键入 "fire" , 点确定
* [6] 接下来触发上一个步骤中添加到工具栏中的项目就可以开火了!!!
* 
* (请注意, 按此步骤设置则采用脚本默认选项, 这通常可用于大多数常规的(机枪分离)火炮, 但仍不能保证适用于所有火炮)
* 
* 
* ============================== 脚本功能 ==============================
* 
* [0] 多联装火炮轮射齐射功能, 可以通过特定参数运行脚本一次进行模式切换
* [1] 本脚本可自定义程度极高, 可自行配置选项以支持各种不同的火炮
* [2] 本脚本可以自动识别并维护多个火炮的运行状态, 脚本会自动停用或者尝试重新复位被检测为故障的火炮
*     脚本识别两种火炮故障类型: 
*     [0] 火炮本身的元件被检测出无法工作(可能是因为元件被关闭, 或元件耐久低于阈值(俗称血条红了) )
*     [1] 火炮蓄力元件(通常是活塞)没有成功附着并蓄力, 此时脚本能够尝试自动重新装载火炮(可在脚本选项中开关此功能)
*     自动检查频率(周期)也可在脚本选项中配置
* [3] 脚本支持自动射击, 通过特定参数运行脚本一次可以开关此功能, 自动射击的频率(周期)可在脚本选项中配置
* [4] 脚本支持LCD信息显示, 脚本可以获取特定编组内的全部LCD显示器, 并且根据其自定义数据显示不同的信息, 
*     输出信息的刷新频率(周期)可在脚本选项中配置
* [5] 火炮编组功能, 脚本支持将数门火炮编入一个组内, 实现高级编组管理
* [6] 弹头高级保险, 脚本支持弹头安全锁, 同时会检查炮弹是否成功分离
* [7] 自定义定时器接口, 可以通过设置自定义定时器来实现一些更加细致的火炮动作(如火炮状态指示灯等等)
* [8] 脚本目前支持两个运行模式: 常规模式和武器同步模式, 适应绝大多数场景
* 
* 
* ============================== 使用方法 ==============================
* 
* [0] 在网格上面放置一个编程块, 把脚本的所有内容粘贴进去, 点击确定之后脚本开始进行初始化
* [1] 脚本会自动在其所在的编程块的自定义数据中生成脚本配置选项, 通过修改这些配置可以高度自定义脚本的行为
* [2] 在脚本选项中设置你想让本脚本管理的活塞火炮编号范围
*     <举例> 如果设置 index__cannon_begin = 0, index__cannon_end = 2 , 
*     则脚本控制的火炮为0号, 1号和2号. 如果都设为0则只控制0号
* [3] 将你的活塞炮的核心元件和一些可选功能的元件按照脚本指定的名称(可在脚本选项中修改)进行编组
*     <举例> 如果设置 tag__cannon_pistons_group = Pistons_#, 则第0号火炮的活塞编组名应该是 Pistons_#0, 
*     第1号火炮的活塞编组名应该是 Pistons_#1, 本脚本完全依赖编组名称的编号后缀来区分不同火炮的同类型组件, 
*     同样的, 若设置 tag__cannon_detachers_group = Gatlins_#, 则第2号火炮的炮弹分离器编组名应该是 Gatlins_#2.
*     有关脚本配置选项的说明, 请查阅"脚本选项"主题
* [4] 如果你顺利地完成了以上步骤并运行了脚本, 此时在编程块的终端中将会显示一些信息, 
*     这些信息能够提示你脚本的运行状态, 比如, 火炮的运行状态等等, 详情请查阅"信息显示"主题
* [5] 当脚本能够正常地识别你的火炮之后, 使用参数运行脚本就可以控制你的火炮了, 详情请查阅"脚本参数"主题
* [6] 最后, 若你想让作品看起来更加炫酷一些, 让脚本将信息实时输出到LCD显示器上会是个不错的选择, 
*     你只需要将LCD按照脚本指定的方式(可在脚本选项中修改)编组, 脚本就能根据你在LCD中输入的自定义数据, 
*     在对应的LCD上显示不同的信息. 有关脚本支持的LCD自定义数据, 请查阅"LCD配置"主题
* 
* 
* ============================== 脚本参数 ==============================
* (使用这些参数运行脚本以实现对应功能)
* 
* [0] "fire"(键入时不含双引号, 下同) 脚本尝试进行一次射击(齐射或轮射, 默认是轮射模式)
* [1] "toggle_inter_group_fire_mode" 脚本切换组间射击模式
* [2] "toggle_intra_group_fire_mode" 脚本切换组内射击模式
* [3] "toggle_auto_fire_onoff"   此参数可以让脚本开关自动射击(自动射击相当于以固定的频率执行 fire 参数)
* 
* 
* ============================== 脚本选项 ==============================
* (你没看错, 选项就是这么多, 这是为了实现标准化...不过, 如果你用的是常规火炮, 其实大部分选项不用修改, 默认就好)
* 
* [0] mode_script                                 脚本全局运行模式, 一般来讲不需要修改
*     脚本的运行模式默认是 Regular, 也即常规模式, 这种模式下, 脚本会响应 fire 命令. 
*     该项的另一个有效值是 WeaponSync, 即, 武器同步模式, 此模式下, 脚本不响应 fire 命令,
*     脚本会为每一个火炮编组查找以指定方式命名的目标武器编组, 
*     若检测到编组内有武器方块(火箭发射器和加特林机枪)处于射击状态, 那么脚本也会控制相应的编组进行射击
* 
* [1] index__cannon_begin                   (重要) 脚本控制的火炮的起始编号(含)
* [2] index__cannon_end                     (重要) 脚本控制的火炮的末尾编号(含)
*      <举例> 若 index__cannon_begin = 0, index__cannon_end = 5, 
*      此时脚本就知道你要控制的火炮有 0 1 2 3 4 5 一共六门. 
*      
* [3] number__cannons_in_group              (重要) 一个编组内的火炮数量
*      若 number__cannons_in_group = 0, 则无论有多少门炮, 都会置入一个编组, 
*      若 number__cannons_in_group = m, 其中 m 不等于0 , 则脚本会按顺序, 
*      将火炮放入编组中, 每个编组 m 个火炮, 对于火炮数量不可整除 m 的情况
*      最后一个编组的火炮数量会低于 m
*      <举例> 若有 0 1 2 3 4 5 六门火炮, 且 number__cannons_in_group = 4, 
*      则 0 1 2 3 为0号编组, 4 5 为1号编组. 
* 
* [4] tag__cannon_pistons_group             (必须) 火炮的核心活塞元件编组名的标签
* [5] tag__cannon_releasers_group           (必须) 火炮的释放和附着元件编组名的标签 (只支持转子)
* [6] tag__cannon_detachers_group           (必须) 火炮的炮弹分离元件编组名的标签 (只支持加特林, 合并块, 和切割机)
* [7] tag__cannon_shell_warheads_group      (可选) 火炮的弹头元件编组名的标签 (用于自动装备弹头)
* [8] tag__cannon_shell_welders_group       (可选) 火炮的焊接器元件编组名的标签 (用于自动开关焊接器)
* [9] tag__cannon_vertical_joints_group     (可选) 火炮的垂直旋转关节元件编组名的标签 (用于自动开关关节锁定)
* [10] tag__postfix_cannon_status_indicator (可选) 火炮的活塞状态指示器的后缀
*     将活塞编组中的某个活塞以该后缀命名后, 脚本会以该活塞的伸缩状态视作其所在火炮的活塞伸缩状态, 
*     如果脚本找不以特定后缀命名的活塞, 就会自动从编组中随机选择(列表第一个)一个活塞作为状态指示器, 
*     请注意, 若你的火炮是常规活塞火炮, 那么是不需要设置这一项的, 
*     这里假设常规火炮的全部活塞状态总是一致(通常这是成立的), 
*     但考虑到某些火炮(例如, 此脚本作者的某些火炮), 其可能存在状态完全相反的活塞, 
*     此时, 脚本随机选择一个活塞作为指示器的方式就可能导致错误
*     
* [11] tag__custom_interface_timer_0        (可选) 火炮的第一个用户自定义接口定时器名称的标签
*     脚本会找到以此标签命名的定时器(请注意, 和诸如 tag__cannon_pistons_group 这种标签一样, 后面需要一个编号后缀), 
*     在火炮开炮的合适阶段(你可以设置该时间)会自动触发该定时器一次, 以这种通用的方式, 
*     你可以将一些你所需要的, 但是脚本未能提供的自定义动作放到这个定时器中, 完成你的特殊需求, 
*     <举例> 比如你需要在火炮开炮的瞬间关闭一个室内灯, 在装填完毕后重新打开它(作为状态指示灯), 那么就需要用到此功能
*     
* [12] tag__custom_interface_timer_1        (可选) 火炮的第二个用户自定义接口定时器名称的标签
*     和上一条相同, 不同的是, 这是第二个定时器接口, 此脚本一共支持两个用户自定义定时器接口
*     
* [13] tag__info_display_LCD_group          (可选) 脚本的信息显示LCD编组
* [14] flag__auto_activate_warhead                 (功能开关) 自动装备弹头
* [15] flag__auto_start_warhead_countdown          (功能开关) 自动开启弹头倒计时
* [16] flag__auto_toggle_welders_onoff             (功能开关) 自动开关焊接器
* [17] flag__auto_toggle_vertical_joints_lock      (功能开关) 自动锁定解锁垂直关节
* [18] flag__auto_trigger_custom_interface_timer_0 (功能开关) 自动触发第一个用户定时器接口
* [19] flag__auto_trigger_custom_interface_timer_1 (功能开关) 自动触发第二个用户定时器接口
* [20] flag__auto_check                            (功能开关) 按指定频率自动检查火炮状态
* [21] flag__auto_reload                           (功能开关) 在火炮出现异常时尝试复位(自动重载)
* [22] period__auto_check                   (周期, 整数) 自动检查周期
* [23] period__auto_fire                    (周期, 整数) 自动射击周期
* [24] period__update_output                (周期, 整数) 更新脚本输出周期(LCD, 编程块终端)
* [25] delay_release                        (延时, 整数) 火炮释放弹性势能的时刻
*      在这一个动作, 如果开启了自动锁定垂直关节功能, 那么脚本会锁定对应的转子或铰链.
*      
* [26] delay__detach_begin                  (延时, 整数) 火炮开始准备进行炮弹分离的时刻
*      在这一个动作, 如果开启了自动激活弹头功能, 那么脚本会开始尝试获取弹头对象.
*      若你的火炮采用加特林分离, 那么此时脚本会尝试打开加特林,
*      若是切割机分离, 脚本会尝试打开切割机(并开始切割)
*      若是合并块分离, 脚本不会有任何动作
*      
* [27] delay_detach                         (延时, 整数) 火炮进行炮弹分离的时刻
*      若你的火炮采用加特林分离, 那么此时脚本会尝试让加特林射击一次,
*      若是切割机分离, 脚本会尝试关闭切割机(并停止切割),
*      若是合并块分离, 脚本会把合并块关闭以实现分离
*      
* [28] delay__detach_end                    (延时, 整数) 火炮结束炮弹分离的时刻
*      在这一个动作, 如果开启了自动激活弹头功能, 那么脚本会开始尝试装备弹头.
*      若指定了分离部件网格定位器, 那么此时脚本还会尝试检查以下两点:
*      [0] 弹头是否成功与定位器所在的网格分离
*      [1] 弹头与定位器之间的距离是否超过安全阈值
*      只有当通过两个检查之后, 脚本才会装备弹头.
*      若你的火炮采用加特林分离, 那么此时脚本会尝试关闭加特林,
*      若是切割机分离, 脚本不会有任何动作,
*      若是合并块分离, 脚本会把合并块重新开启, 为下一轮焊接做准备
*      
* [29] delay__pistons_extend                (延时, 整数) 火炮活塞伸展的时刻
*      在这一个动作, 如果开启了自动开关焊接器, 那么脚本会尝试打开焊接器.
*      
* [30] delay_attach                         (延时, 整数) 火炮机构附着的时刻
*      在这一个动作, 如果开启了自动锁定垂直关节功能, 那么脚本会解锁对应的转子或铰链.
*      
* [31] dealy__pistons_retract               (延时, 整数) 火炮活塞收缩(蓄力)的时刻
* [32] delay__custom_interface_timer_0      (延时, 整数) 触发第一个用户自定义定时器的时刻
* [33] delay__custom_interface_timer_1      (延时, 整数) 触发第二个用户自定义定时器的时刻
* [34] delay__done_loading                  (延时, 整数) 火炮装填完成的时刻
*      在这一个动作, 如果开启了自动开关焊接器, 那么脚本会尝试关闭焊接器.
*      
* [35] distance__warhead_savety_lock       (距离, 浮点数) 弹头安全锁距离
* [36] number__warhead_countdown_seconds    (秒数, 浮点数) 弹头倒计时秒数
* 
* ============================== LCD配置 ==============================
* (请在LCD的自定义数据中键入以下配置)
* 
* [0] "general"                  显示脚本运行状态信息和一些实用的统计信息
* [1] "" (什么都不写)            显示脚本运行状态信息和一些实用的统计信息(同上)
* [2] "cannon m"                 显示编号为 m 的火炮的详细信息
*     <举例> 若LCD的自定义数据为 "cannon 1", 则显示1号火炮的详细信息
* [3] "cannon m n"               显示编号在 m(含) 到 n(含) 范围内的火炮的简略信息
*     <举例> 自定义数据为 "cannon 0 2"(不包含双引号, 下同), 则显示0号, 1号和2号火炮的简略信息
* [4] "group m"                  显示编号为 m 的编组的信息
*     <举例> 自定义数据为 "group 1", 则显示1号编组的信息
* [5] "group m n"                显示编号在 m(含) 到 n(含) 范围内的编组的信息
*     <举例> 自定义数据为 "group 1 2", 则显示1号和2号编组的信息
* [6] 注: 
*     [0] 若在以上指令末尾添加 "graphic"(空格分隔), 脚本将以图形方式显示信息
*         只有以上 [2] [3] [4] 条目的配置支持图形信息显示
*         但不推荐使用这个功能, 这将加重CPU和显卡的负担, 并且可能导致转子出现BUG
*     [1] 若脚本发现非法的LCD自定义数据, 将会显示错误信息
* 
* 
* ============================== 信息显示 ==============================
* 
* [0] 这个部分我懒得写了, 请自行尝试理解一下脚本的各种信息输出
* 
* 
* ============================== 注意事项 ==============================
* 
* [0] 修改脚本的任何配置选项之后, 请重置代码一次来应用这些更改
* [1] 出于某些原因, 此脚本的刷新频率是固定每帧一次
* [2] 不建议修改脚本代码内容, 以免出现故障和异常. (不过我写了注释, 有能力可自行修改)
* 
* 
* ============================== 更新日志 ==============================
* 
* ========== V0.0.4 ==========
* 常规更新
* [0] 使用了新的(重写的)脚本配置通用代码,
*     现在会在编程块命令行低端显示脚本配置中出现的错误
* [1] 编程块输出信息格式微调, 现在会显示脚本版本
* [2] 脚本代码微调, 删掉了部分多余的代码, 和不优雅的结构
* 额外消息
* [0] 最新的SE疑似修复了转子BUG, 现在你可以尝试开启图形化信息显示功能,
*     如果出现意外情况请你关闭该功能
* 
* ========== V0.0.3 ==========
* 新功能
* [0] 增加了LCD图形化信息显示(会增加CPU和显卡的负担, 不推荐使用)
* 常规更新
* [0] 优化了代码结构
* 已知问题
* [0] 疑似在游戏卡顿(脚本卡顿)的情况下, 容易导致火炮的分离转子出现BUG状态, 
*     目前, 出现这种现象的原因尚不明, 极有可能是SE的BUG, 
*     我花了很长时间仔细查看与转子有关的接口, 都未能发现脚本的任何问题
* 
* ========== V0.0.2 ==========
* 常规更新
* [0] 现在会为每一个弹头检查保险
* 
* ========== V0.0.1 ==========
* 新功能
* [0] 新增了自动开启炮弹弹头倒计时的功能, 可在选项中关闭此功能(默认开启)
* 
* ========== V0.0.0 ==========
* 初始版本
* 
* 
********************************************************************************************************************************************************************************************************/

#region using 声明

//用于IDE开发的 using 声明, 脚本运行时请注释掉这一段代码
//using Sandbox.ModAPI.Ingame;
//using SpaceEngineers.Game.ModAPI.Ingame;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using VRage.Game.GUI.TextPanel;
//using VRage.Game.ModAPI.Ingame;
//using VRageMath;

#endregion

#region 脚本字段

//数组 运行时字符显示
string[] array__runtime_chars = new string[]
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

//标记(全局) 是否 自动激活弹头
bool flag__auto_activate_warhead = true;
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

//间隔 自动检查(默认每秒检查一次)
int period__auto_check = 60;
//间隔 自动射击(默认每秒射击一次)
int period__auto_fire = 60;
//间隔 更新输出(默认每秒3次)
int period__update_output = 20;

//延时 释放(默认无延时)
int delay_release = 1;
//延时 分离开始(默认无延时)
int delay__detach_begin = 1;
//延时 分离(默认2帧)
int delay_detach = 2;
//延时 分离结束(默认10帧)
int delay__detach_end = 10;
//延时 伸展(默认无延时)
int delay__pistons_extend = 1;
//延时 固定(默认80帧后附着)
int delay_attach = 80;
//延时 收缩(默认120帧后收缩)
int dealy__pistons_retract = 120;
//延时 定时器 用户自定义接口0(默认无延时)
int delay__custom_interface_timer_0 = 1;
//延时 定时器 用户自定义接口1(默认180帧)
int delay__custom_interface_timer_1 = 180;
//延时 完成装填(默认180帧)
int delay__done_loading = 180;

//距离 弹头安全锁(默认50米)
float distance__warhead_savety_lock = 50.0F;
//数量 倒计时秒数
float number__warhead_countdown_seconds = 30.0F;

//列表 活塞炮
List<PistonCannon> list__piston_cannons = new List<PistonCannon>();
//列表 火炮编组
List<CannonGroup> list__cannon_groups = new List<CannonGroup>();
//列表 显示器
List<IMyTextPanel> list_displayers = new List<IMyTextPanel>();
//列表 显示单元
List<DisplayUnit> list__display_units = new List<DisplayUnit>();

//索引 上一次射击的编组
int index__group_last_fire = -1;

//延迟 自动射击前的延迟
int delay__before_auto_fire = 5;

//枚举变量 脚本运行模式(默认常规模式)
ScriptMode mode_script = ScriptMode.Regular;
//枚举变量 组间射击模式(默认齐射)
FireMode mode_fire__inter_group = FireMode.Salvo;
//枚举变量 组内射击模式(默认轮射)
FireMode mode_fire__intra_group = FireMode.Round;

//计数 脚本触发次数(工具栏手动触发次数)
long count_trigger = 0;
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

//字符串构建器 默认信息
StringBuilder string_builder__default_info = new StringBuilder();
//字符串构建器 测试信息
StringBuilder string_builder__test_info = new StringBuilder();
//脚本配置
CustomDataConfig config_script = null;

#endregion

#region 脚本入口

/***************************************************************************************************
* 构造函数 Program()
***************************************************************************************************/
public Program()
{
	Echo("<execute> init");

	//初始化脚本配置
	init_script_config();

	//检查配置合法性
	string str_error;
	if (!check_config(out str_error))
	{
		Echo(str_error);
		return;
	}

	//初始化脚本
	init_script();

	//设置执行频率 每1帧一次
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
	
	Echo("<done> init");
	return;
}

/***************************************************************************************************
* 入口函数 main()
***************************************************************************************************/
void Main(string str_arg, UpdateType type_update)
{
	if(type_update == UpdateType.Trigger)
	{
		//工具栏带参数执行
		//检查命令
		switch (str_arg)
		{
		case "fire"://开火
			{
				fire();
			}
			break;
		case "toggle_inter_group_fire_mode"://切换组间射击模式
			{
				if (mode_fire__inter_group == FireMode.Salvo)
					mode_fire__inter_group = FireMode.Round;
				else
					mode_fire__inter_group = FireMode.Salvo;
			}
			break;
		case "toggle_intra_group_fire_mode"://切换组内射击模式
			{
				if (mode_fire__intra_group == FireMode.Salvo)
					mode_fire__intra_group = FireMode.Round;
				else
					mode_fire__intra_group = FireMode.Salvo;
				foreach (var item in list__cannon_groups)
					item.set_group_fire_mode(mode_fire__intra_group);
			}
			break;
		case "toggle_auto_fire_onoff":
			{
				flag__auto_fire = !flag__auto_fire;
				//设置自动射击前的延迟
				times__before_next_auto_fire = delay__before_auto_fire;
			}
			break;
			
		}
		//触发计数+1
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
	if (mode_script == ScriptMode.WeaponSync)
	{
		//武器同步模式
		//检查所有群组
		flag__auto_fire = false;
		foreach (var item in list__cannon_groups)
			if (item.check_and_update_weapon_synchronization_status())
				flag__auto_fire = true;
	}

	//自动射击
	if (flag__auto_fire)
	{
		if (times__before_next_auto_fire == 0)
		{
			times__before_next_auto_fire = period__auto_fire;
			fire();
		}
		--times__before_next_auto_fire;
	}
	//更新所有火炮状态
	foreach (var item in list__piston_cannons)
		item.update();
	//信息显示
	display_info();
	//更新次数+1
	++count_update;
}

//输出信息 输出信息到编程块终端和LCD
void display_info()
{
	if(times__before_next_output_update != 0)
	{
		--times__before_next_output_update;
		return;
	}
	else
		times__before_next_output_update = period__update_output;
	--times__before_next_output_update;

	//清空
	string_builder__default_info.Clear();

	string_builder__default_info.Append(
		"<script> AMPCCS V0.0.4 " + array__runtime_chars[index__crt_char_pattern]
		+ "\n<mode_script>" + mode_script
		+ "\n<mode_fire__inter_group>" + mode_fire__inter_group
		+ "\n<mode_fire__intra_group>" + mode_fire__intra_group
		+ "\n<count_update> " + count_update
		+ "\n<count_trigger> " + count_trigger
		+ "\n<count_fire> " + count_fire
		+ "\n<flag__auto_fire> " + flag__auto_fire
		+ "\n<count_groups> total " + list__cannon_groups.Count
		+ "\n<count_cannons> total " + list__piston_cannons.Count
		);

	//更新动态字符图案
	if (times__before_next_char_pattern_update == 0)
	{
		times__before_next_char_pattern_update = 1;
		++index__crt_char_pattern;
		if (index__crt_char_pattern >= array__runtime_chars.Length)
			index__crt_char_pattern = 0;
	}
	--times__before_next_char_pattern_update;

	Echo(string_builder__default_info.ToString());

	foreach(var item in list__cannon_groups)
	{
		Echo(item.get_group_info());
	}

	//遍历显示单元
	foreach (var item in list__display_units)
	{
		if (item.flag_graphic)
		{
			//图形化显示

			if (item.lcd.ContentType != ContentType.SCRIPT)
				continue;

			switch (item.mode_display)
			{
			case DisplayUnit.DisplayMode.General:
				draw_illegal_lcd_custom_data_hint(item.lcd);
				break;
			case DisplayUnit.DisplayMode.SingleCannon:
				draw_cannons_state(item.lcd, item.index_begin, item.index_begin);
				break;
			case DisplayUnit.DisplayMode.MultipleCannon:
				draw_cannons_state(item.lcd, item.index_begin, item.index_end);
				break;
			case DisplayUnit.DisplayMode.SingleGroup:
				draw_cannons_state(item.lcd,
					list__cannon_groups[item.index_begin].get__index_begin(),
					list__cannon_groups[item.index_begin].get__index_end());
				break;
			case DisplayUnit.DisplayMode.MultipleGroup:
				{

				}
				break;
			case DisplayUnit.DisplayMode.None:
				draw_illegal_lcd_custom_data_hint(item.lcd);
				break;
			}
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
			case DisplayUnit.DisplayMode.SingleCannon:
				item.lcd.WriteText(
					list__piston_cannons[item.index_begin].get_cannon_LCD_info());
				break;
			case DisplayUnit.DisplayMode.MultipleCannon:
				{
					StringBuilder sb_temp = new StringBuilder();
					for (var i = item.index_begin; i <= item.index_end; ++i)
						sb_temp.Append(list__piston_cannons[i].get_cannon_simple_info() + "\n");
					item.lcd.WriteText(sb_temp);
				}
				break;
			case DisplayUnit.DisplayMode.SingleGroup:
				item.lcd.WriteText(list__cannon_groups[item.index_begin].get_group_LCD_info());
				break;
			case DisplayUnit.DisplayMode.MultipleGroup:
				{
					StringBuilder sb_temp = new StringBuilder();
					for (var i = item.index_begin; i <= item.index_end; ++i)
						sb_temp.Append(list__cannon_groups[i].get_group_LCD_info() + "\n");
					item.lcd.WriteText(sb_temp);
				}
				break;
			case DisplayUnit.DisplayMode.None:
				item.lcd.WriteText("<warning> illegal custom data in this LCD\n<by> script AMPCCS");
				break;
			}
		}
			
	}

	if (config_script.flag__config_error_in_CD)
		Echo("\n<info_config>\n" + config_script.string_builder__error_info.ToString() + "\n");

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
		Type = SpriteType.TEXTURE,
		Data = "CircleHollow",
		Position = size * (new Vector2(0.5f, 0.4f)),
		Size = size * 0.6f,
		Color = new Color(255, 0, 0),
		Alignment = TextAlignment.CENTER,
	};
	frame.Add(element__red_annulus);

	//绘图元素 红色叉
	MySprite element__red_cross = new MySprite()
	{
		Type = SpriteType.TEXTURE,
		Data = "Cross",
		Position = size * (new Vector2(0.5f, 0.4f)),
		Size = size * 0.4f,
		Color = new Color(255, 0, 0),
		Alignment = TextAlignment.CENTER,
	};
	frame.Add(element__red_cross);

	//白色文字
	MySprite element__text = new MySprite()
	{
		Type = SpriteType.TEXT,
		Data = "<warning> illegal custom data in this LCD\n<by> script AMPCCS",
		Position = size * (new Vector2 (0.5f, 0.8f)),
		Color = Color.White,
		Alignment = TextAlignment.CENTER,
		FontId = "White",
		RotationOrScale = 1.0f,
	};
	frame.Add(element__text);

	//刷新缓冲区
	frame.Dispose();

}

//绘制 火炮状态
void draw_cannons_state(IMyTextSurface surface, int index_begin, int index_end)
{
	//帧缓冲区
	MySpriteDrawFrame frame = surface.DrawFrame();

	//大小 显示屏
	Vector2 size = surface.SurfaceSize;
	//偏移量 条
	Vector2 offset_bar = size;
	//偏移量 编号
	Vector2 offset_no = size;
	//大小 条形
	Vector2 size_rect = size;

	//增量 偏移量y轴方向
	float increment__offset_y = size.Y * 0.15f;

	size_rect.X *= 0.7f;
	size_rect.Y *= 0.1f;

	offset_bar.X *= 0.25f;
	offset_bar.Y *= 0.10f;

	offset_no.X *= 0.05f;
	offset_no.Y *= 0.02f;

	for (int index = index_begin; index <= index_end; ++index)
	{
		MySprite element_bar = new MySprite()
		{
			Type = SpriteType.TEXTURE,
			Data = "SquareSimple",
			Position = offset_bar,
			Size = size_rect,
			Color = Color.White,
			FontId = "Monospace",
			Alignment = TextAlignment.LEFT,
		};
		//根据状态调整颜色
		switch (list__piston_cannons[index].status_cannon)
		{
		case PistonCannon.CannonStatus.Ready://就绪
			element_bar.Color = Color.Green;//绿色
			break;
		case PistonCannon.CannonStatus.Loading://装填中
			element_bar.Color = Color.White;//白色
			element_bar.Size = size_rect * (new Vector2((float)list__piston_cannons[index].count_status / delay__done_loading, 1.0f));
			break;
		case PistonCannon.CannonStatus.BrokenDown://故障
			element_bar.Color = Color.Red;//红色
			break;
		case PistonCannon.CannonStatus.Invalid://不可用
			element_bar.Color = Color.Yellow;//黄色
			break;
		}
		frame.Add(element_bar);

		MySprite element_no = new MySprite()
		{
			Type = SpriteType.TEXT,
			Data = "#"+(index + index__cannon_begin),
			Position = offset_no,
			Color = Color.White,
			Alignment = TextAlignment.LEFT,
			FontId = "White",
			RotationOrScale = 2.5f,
		};
		frame.Add(element_no);

		//向下偏移
		offset_bar.Y += increment__offset_y;
		offset_no.Y += increment__offset_y;
	}
	frame.Dispose();
	return;
}

//射击控制
void fire()
{
	if (mode_fire__inter_group == FireMode.Salvo)
	{
		//齐射模式
		//所有火炮编组尝试射击
		fire_salvo();
	}
	else if (mode_fire__inter_group == FireMode.Round)
	{
		//轮射模式
		//上次开火的火炮之后第一门处于就绪状态的火炮进行射击
		fire_round();
	}
}

//齐射
void fire_salvo()
{
	//所有编组尝试射击
	foreach (var item in list__cannon_groups)
		item.fire();
	return;
}

//轮射
void fire_round()
{
	int index = -1;

	//从上一次开炮的位置往后查找到末尾
	int index__last_next = index__group_last_fire + 1;
	for (index = index__last_next; index < list__cannon_groups.Count; ++index)
	{
		//是否成功执行射击
		if (list__cannon_groups[index].fire() != 0)
			break;
	}
	if (index == list__cannon_groups.Count)
	{
		//没有找到
		//从开头重新查找到上次开炮的位置
		for (index = 0; index < index__last_next; ++index)
		{
			//是否成功执行射击
			if (list__cannon_groups[index].fire() != 0)
				break;
		}
	}
	return;
}

//初始化脚本
void init_script()
{
	//计算编组数量
	int num_groups = 1, num_cannons = index__cannon_end - index__cannon_begin + 1;
	int num__cannons_in_group = number__cannons_in_group;
	if (num__cannons_in_group != 0)
	{
		if (num_cannons % num__cannons_in_group == 0)
			num_groups = num_cannons / num__cannons_in_group;
		else
			num_groups = num_cannons / num__cannons_in_group + 1;
	}
	else
		num__cannons_in_group = num_cannons;
	//创建火炮编组对象 并添加到列表
	for (int i = 0; i < num_groups; ++i)
	{
		CannonGroup ptr = new CannonGroup(this, i);
		list__cannon_groups.Add(ptr);
	}
	//创建火炮对象 并添加到列表
	for (int i = index__cannon_begin, index_group = 0; i <= index__cannon_end; ++i)
	{
		index_group = (i - index__cannon_begin) / num__cannons_in_group;
		PistonCannon ptr_temp = new PistonCannon
			(this, i, list__cannon_groups[index_group]);
		//添加到列表和编组中
		list__piston_cannons.Add(ptr_temp);
		list__cannon_groups[index_group].add_cannon(ptr_temp);
	}

	//获取LCD编组
	IMyBlockGroup group_temp = GridTerminalSystem.GetBlockGroupWithName(tag__info_display_LCD_group);

	if (group_temp != null)
		//获取编组中的显示屏
		group_temp.GetBlocksOfType<IMyTextPanel>(list_displayers);

	//遍历显示器
	foreach (var item in list_displayers)
	{
		//拆分显示器的用户数据
		string[] array_str = split_string(item.CustomData);
		bool flag_illegal = false;
		//bool flag_graphic = false;
		int offset = 0;

		DisplayUnit unit = new DisplayUnit(item);

		if (array_str.Length == 0)
			//用户数据为空
			unit.mode_display = DisplayUnit.DisplayMode.General;
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
			case "general":
				unit.mode_display = DisplayUnit.DisplayMode.General;
				break;
			case "cannon":
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
						if (index < index__cannon_begin || index > index__cannon_end)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index - index__cannon_begin;
						unit.mode_display = DisplayUnit.DisplayMode.SingleCannon;
					}
					else if(array_str.Length == 3 + offset)
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
						if (index_begin < index__cannon_begin || index_begin > index__cannon_end)
						{
							flag_illegal = true;
							break;
						}
						if (index_end < index__cannon_begin || index_end > index__cannon_end)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index_begin - index__cannon_begin;
						unit.index_end = index_end - index__cannon_begin;
						unit.mode_display = DisplayUnit.DisplayMode.MultipleCannon;
					}
					else
						flag_illegal = true;
				}
				break;
			case "group":
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
						if (index < 0 || index >= list__cannon_groups.Count)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index;
						unit.mode_display = DisplayUnit.DisplayMode.SingleGroup;
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
						if (index_begin < 0 || index_begin >= list__cannon_groups.Count)
						{
							flag_illegal = true;
							break;
						}
						if (index_end < 0 || index_end >= list__cannon_groups.Count)
						{
							flag_illegal = true;
							break;
						}
						unit.index_begin = index_begin;
						unit.index_end = index_end;
						unit.mode_display = DisplayUnit.DisplayMode.MultipleGroup;
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
		{
			item.ContentType = ContentType.TEXT_AND_IMAGE;//设置为 文字与图片模式
		}

		//添加到列表
		list__display_units.Add(unit);
	}

	//变量初始化
	times__before_next_auto_fire = delay__before_auto_fire;

	return;
}

//检查脚本配置
bool check_config(out string info)
{
	info = "";

	//检查必要编组的标签
	if (tag__cannon_pistons_group.Length == 0
		|| tag__cannon_releasers_group.Length == 0
		|| tag__cannon_detachers_group.Length == 0)
	{
		info = "<error_config> key tag of group name is empty";
		return false;
	}
		
	//检查火炮编号
	if (index__cannon_begin < 0 || index__cannon_end < 0
		|| index__cannon_begin > 1000 || index__cannon_end > 1000
		|| index__cannon_begin > index__cannon_end)
	{
		info = "<error_config> illegal indexes of cannons";
		return false;
	}

	//检查组内火炮数量
	if(number__cannons_in_group <0 || number__cannons_in_group >1000)
	{
		info =
			"<error_config> number of cannons in a group " +
			"cannot be less than 0 or more than 1000";
		return false;
	}

	if (!check_value(period__auto_check)
		|| !check_value(period__auto_fire)
		|| !check_value(period__update_output))
	{
		info = "<error_config> period cannot be less than 1 or more than 1000";
		return false;
	}

	if (!check_value(delay_release)
		|| !check_value(delay__detach_begin)
		|| !check_value(delay_detach)
		|| !check_value(delay__detach_end)
		|| !check_value(delay__pistons_extend)
		|| !check_value(delay_attach)
		|| !check_value(dealy__pistons_retract)
		|| !check_value(delay__custom_interface_timer_0)
		|| !check_value(delay__custom_interface_timer_1)
		|| !check_value(delay__done_loading)
		)
	{
		info = "<error_config> delay cannot be less than 1 or more than 1000";
		return false;
	}

	if (number__warhead_countdown_seconds < 0.0 || number__warhead_countdown_seconds > 300.0)
	{
		info = "<error_config> countdown cannot be less than 0.0 or more than 300.0";
		return false;
	}

	return true;
}

//检查数值
bool check_value(int value)
{
	return value > 0 && value < 1001;
}

//拆分字符串
string[] split_string(string str)
{
	string[] list_str = str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
	return list_str;
}

void init_script_config()
{
	//脚本配置实例
	config_script = new CustomDataConfig(Me);

	//添加配置项

	config_script.add_config_item(nameof(mode_script), () => mode_script, x => { mode_script = (ScriptMode)x; });

	config_script.add_config_item(nameof(index__cannon_begin), () => index__cannon_begin, x => { index__cannon_begin = (int)x; });
	config_script.add_config_item(nameof(index__cannon_end), () => index__cannon_end, x => { index__cannon_end = (int)x; });
	config_script.add_config_item(nameof(number__cannons_in_group), () => number__cannons_in_group, x => { number__cannons_in_group = (int)x; });

	config_script.add_config_item(nameof(tag__cannon_pistons_group), () => tag__cannon_pistons_group, x => { tag__cannon_pistons_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_releasers_group), () => tag__cannon_releasers_group, x => { tag__cannon_releasers_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_detachers_group), () => tag__cannon_detachers_group, x => { tag__cannon_detachers_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_shell_warheads_group), () => tag__cannon_shell_warheads_group, x => { tag__cannon_shell_warheads_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_detach_part_grid_locator), () => tag__cannon_detach_part_grid_locator, x => { tag__cannon_detach_part_grid_locator = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_shell_welders_group), () => tag__cannon_shell_welders_group, x => { tag__cannon_shell_welders_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_vertical_joints_group), () => tag__cannon_vertical_joints_group, x => { tag__cannon_vertical_joints_group = (string)x; });
	config_script.add_config_item(nameof(tag__cannon_group_fire_indicators_group), () => tag__cannon_group_fire_indicators_group, x => { tag__cannon_group_fire_indicators_group = (string)x; });
	config_script.add_config_item(nameof(tag__postfix_cannon_status_indicator), () => tag__postfix_cannon_status_indicator, x => { tag__postfix_cannon_status_indicator = (string)x; });
	config_script.add_config_item(nameof(tag__custom_interface_timer_0), () => tag__custom_interface_timer_0, x => { tag__custom_interface_timer_0 = (string)x; });
	config_script.add_config_item(nameof(tag__custom_interface_timer_1), () => tag__custom_interface_timer_1, x => { tag__custom_interface_timer_1 = (string)x; });
	config_script.add_config_item(nameof(tag__info_display_LCD_group), () => tag__info_display_LCD_group, x => { tag__info_display_LCD_group = (string)x; });

	config_script.add_config_item(nameof(flag__auto_activate_warhead), () => flag__auto_activate_warhead, x => { flag__auto_activate_warhead = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_start_warhead_countdown), () => flag__auto_start_warhead_countdown, x => { flag__auto_start_warhead_countdown = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_toggle_welders_onoff), () => flag__auto_toggle_welders_onoff, x => { flag__auto_toggle_welders_onoff = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_toggle_vertical_joints_lock), () => flag__auto_toggle_vertical_joints_lock, x => { flag__auto_toggle_vertical_joints_lock = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_trigger_custom_interface_timer_0), () => flag__auto_trigger_custom_interface_timer_0, x => { flag__auto_trigger_custom_interface_timer_0 = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_trigger_custom_interface_timer_1), () => flag__auto_trigger_custom_interface_timer_1, x => { flag__auto_trigger_custom_interface_timer_1 = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_check), () => flag__auto_check, x => { flag__auto_check = (bool)x; });
	config_script.add_config_item(nameof(flag__auto_reload), () => flag__auto_reload, x => { flag__auto_reload = (bool)x; });

	config_script.add_config_item(nameof(period__auto_check), () => period__auto_check, x => { period__auto_check = (int)x; });
	config_script.add_config_item(nameof(period__auto_fire), () => period__auto_fire, x => { period__auto_fire = (int)x; });
	config_script.add_config_item(nameof(period__update_output), () => period__update_output, x => { period__update_output = (int)x; });
	config_script.add_config_item(nameof(delay_release), () => delay_release, x => { delay_release = (int)x; });
	config_script.add_config_item(nameof(delay__detach_begin), () => delay__detach_begin, x => { delay__detach_begin = (int)x; });
	config_script.add_config_item(nameof(delay_detach), () => delay_detach, x => { delay_detach = (int)x; });
	config_script.add_config_item(nameof(delay__detach_end), () => delay__detach_end, x => { delay__detach_end = (int)x; });
	config_script.add_config_item(nameof(delay__pistons_extend), () => delay__pistons_extend, x => { delay__pistons_extend = (int)x; });
	config_script.add_config_item(nameof(delay_attach), () => delay_attach, x => { delay_attach = (int)x; });
	config_script.add_config_item(nameof(dealy__pistons_retract), () => dealy__pistons_retract, x => { dealy__pistons_retract = (int)x; });
	config_script.add_config_item(nameof(delay__custom_interface_timer_0), () => delay__custom_interface_timer_0, x => { delay__custom_interface_timer_0 = (int)x; });
	config_script.add_config_item(nameof(delay__custom_interface_timer_1), () => delay__custom_interface_timer_1, x => { delay__custom_interface_timer_1 = (int)x; });
	config_script.add_config_item(nameof(delay__done_loading), () => delay__done_loading, x => { delay__done_loading = (int)x; });

	config_script.add_config_item(nameof(distance__warhead_savety_lock), () => distance__warhead_savety_lock, x => { distance__warhead_savety_lock = (float)x; });
	config_script.add_config_item(nameof(number__warhead_countdown_seconds), () => number__warhead_countdown_seconds, x => { number__warhead_countdown_seconds = (float)x; });

	//初始化配置
	config_script.init_config();

	return;
}

#endregion

#region 类型定义

//枚举 射击模式
public enum FireMode
{
	Salvo,//齐射
	Round,//轮射
}

//脚本运行模式
public enum ScriptMode
{
	Regular,//常规模式
	WeaponSync,//武器同步模式
}

//类 显示单元(当成结构体用)
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
		lcd = _lcd;
		index_begin = _index_begin;
		index_end = _index_end;
		flag_graphic = _flag_graphic;
		return;
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

	//火炮所属的组
	CannonGroup group;

	//编组 活塞元件
	IMyBlockGroup group_pistons = null;// "= null" 可以不写....
	//编组 释放元件
	IMyBlockGroup group_releasers;
	//编组 分离元件
	IMyBlockGroup group_detachers;
	//编组 弹头元件
	IMyBlockGroup group_warheads;
	//编组 焊接器元件
	IMyBlockGroup group_welders;
	//编组 垂直关节元件
	IMyBlockGroup group__vertical_joints;

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

	//活塞 状态指示器 (火炮的拉伸, 收缩状态以其为参照)
	//当编组中没有以指定规则命名的活塞时, 选取编组中的第一根活塞作为指示器
	IMyPistonBase piston__status_indicator;

	//转子 状态指示器 (转子是否成功附着以其为参照)
	//选择释放元件的第一个转子作为指示器
	IMyMechanicalConnectionBlock rotor__status_indicator;

	//定时器 用户定时器接口0
	IMyTimerBlock timer__custom_interface_0;

	//定时器 用户定时器接口1
	IMyTimerBlock timer__custom_interface_1;

	//定位器 火炮分离部件网格
	IMyTerminalBlock locator__cannon_detach_part_grid;

	//网格 上一个炮弹
	IMyCubeGrid grid__previous_shell;

	//索引 活塞炮的编号
	public int index_cannon { get; private set; } = -1;

	//编程块的this指针
	Program program;

	//字符串构建器 对象初始化时的信息
	StringBuilder string_builder__init_info = new StringBuilder();

	//标记 是否 自动激活弹头
	bool flag__auto_activate_warhead = true;
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

	//次数 距离下一次 检查
	int times__before_next_check = 0;

	//计数 状态计数
	public int count_status { get; private set; } = 1;

	//状态 火炮状态
	public CannonStatus status_cannon { get; private set; } = CannonStatus.NotReady;
	//指令 火炮操作指令
	public CannonCommand command_cannon { get; private set; } = CannonCommand.None;
	//模式 分离模式
	DetachMode mode_detach = DetachMode.None;

	//构造函数(传递函数指针以及火炮索引)
	public PistonCannon(Program _program, int _index, CannonGroup _group)
	{
		//对成员进行赋值
		this.program = _program;
		this.index_cannon = _index;
		this.group = _group;

		//字符串 临时名称拼接 C#的字符串大概和java一样吧(不管就当是一样的)
		string name_group;

		//清空字符串构建器
		string_builder__init_info.Clear();

		//初始化(获取控制过程中将使用的全部方块对象)

		/********************
		* 获取活塞元件
		********************/
		//拼接字符串, 得到编组名称
		name_group = this.program.tag__cannon_pistons_group + this.index_cannon;
		//获取活塞元件编组
		group_pistons = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if(group_pistons == null)
		{
			string_builder__init_info.Append("<error> no group found with name \"" + name_group + "\"\n");
			status_cannon = CannonStatus.Invalid;
		}
		else
		{
			//添加到列表
			group_pistons.GetBlocksOfType<IMyPistonBase>(list_pistons);
			//检查
			if(list_pistons.Count == 0)
			{
				string_builder__init_info.Append("<warning> no any piston found in group \"" + name_group + "\"\n");
				status_cannon = CannonStatus.Invalid;
			}
			else
			{
				//逐个遍历活塞列表, 查找名称以指定规则结尾的活塞
				foreach (var item in list_pistons)
				{
					if (item.CustomName.EndsWith(program.tag__postfix_cannon_status_indicator))
					{
						//设置指示器
						piston__status_indicator = item;
					}
				}
				//如果 没有找到以指定后缀命名的活塞
				if (piston__status_indicator == null)
				{
					//将指示器设置为列表中第一个活塞
					piston__status_indicator = list_pistons[0];
				}
			}
		}

		/********************
		* 获取释放元件
		********************/
		//拼接字符串, 得到编组名称
		name_group = this.program.tag__cannon_releasers_group + this.index_cannon;
		//获取活塞元件编组
		group_releasers = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if(group_releasers == null)
		{
			string_builder__init_info.Append("<error> no group found with name \"" + name_group + "\"\n");
			status_cannon = CannonStatus.Invalid;
		}
		else
		{
			//添加到列表
			group_releasers.GetBlocksOfType<IMyMechanicalConnectionBlock>(list_releasers);
			//检查
			if (list_releasers.Count == 0)
			{
				string_builder__init_info.Append("<warning> no any rotor or hinge found in group \"" + name_group + "\"\n");
				status_cannon = CannonStatus.Invalid;
			}
			else
				//将第一个设为释放元件指示器
				rotor__status_indicator = list_releasers[0];
		}

		/********************
		* 获取分离元件
		********************/
		//拼接字符串, 得到编组名称
		name_group = this.program.tag__cannon_detachers_group + this.index_cannon;
		//获取活塞元件编组
		group_detachers = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if (group_detachers == null)
		{
			string_builder__init_info.Append("<error> no group found with name \"" + name_group + "\"\n");
			status_cannon = CannonStatus.Invalid;
		}
		else
		{
			//添加到列表
			group_detachers.GetBlocksOfType<IMyFunctionalBlock>(list_detachers);
			//检查
			if (list_releasers.Count == 0)
			{
				string_builder__init_info.Append("<warning> no gatlin or merge bLock found in group \"" + name_group + "\"\n");
				status_cannon = CannonStatus.Invalid;
			}
			else
			{
				var temp = list_detachers[0];

				//自动判断分离模式, 使用列表的第一个对象的类型进行判断
				if (temp is IMySmallGatlingGun)
					mode_detach = DetachMode.GatlinDestroy;
				else if (temp is IMyShipMergeBlock)
					mode_detach = DetachMode.Normal;
				else if (temp is IMyShipGrinder)
					mode_detach = DetachMode.GrinderDestroy;
				else
					//脚本不支持的分离模式
					status_cannon = CannonStatus.Invalid;
			}
		}


		/********************
		* 获取分离部件网格定位器
		********************/
		name_group = this.program.tag__cannon_detach_part_grid_locator + this.index_cannon;
		locator__cannon_detach_part_grid = program.GridTerminalSystem.GetBlockWithName(name_group);
		if (locator__cannon_detach_part_grid == null)
			string_builder__init_info.Append("<warning> no block found with name \"" + name_group + "\" , ignored\n");

		/********************
		* 获取焊接器元件
		********************/
		//拼接字符串, 得到编组名称
		name_group = this.program.tag__cannon_shell_welders_group + this.index_cannon;
		//获取活塞元件编组
		group_welders = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if (group_welders == null)
		{
			string_builder__init_info.Append("<warning> no group found with name \"" + name_group + "\" , ignored\n");
		}
		else
		{
			//添加到列表
			group_welders.GetBlocksOfType<IMyShipWelder>(list_welders);
			//检查
			if (list_welders.Count == 0)
				string_builder__init_info.Append("<warning> no any welder found in group \"" + name_group + "\"\n");
		}

		/********************
		* 获取垂直关节元件
		********************/
		//拼接字符串, 得到编组名称
		name_group = this.program.tag__cannon_vertical_joints_group + this.index_cannon;
		//获取活塞元件编组
		group__vertical_joints = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if(group__vertical_joints == null)
		{
			string_builder__init_info.Append("<warning> no group found with name \"" + name_group + "\" , ignored\n");
		}
		else
		{
			//添加到列表
			group__vertical_joints.GetBlocksOfType<IMyMotorStator>(list__vertical_joints);
			//检查
			if(list__vertical_joints.Count == 0)
				string_builder__init_info.Append("<warning> no any rotor or hinge found in group \"" + name_group + "\"\n");
		}

		/********************
		* 获取用户自定义接口定时器
		********************/
		//接口0
		name_group = this.program.tag__custom_interface_timer_0 + this.index_cannon;
		timer__custom_interface_0 = program.GridTerminalSystem.GetBlockWithName(name_group) as IMyTimerBlock;
		if(timer__custom_interface_0 == null)
			string_builder__init_info.Append("<warning> no timer found with name \"" + name_group + "\" , ignored\n");

		//接口1
		name_group = this.program.tag__custom_interface_timer_1 + this.index_cannon;
		timer__custom_interface_1 = program.GridTerminalSystem.GetBlockWithName(name_group) as IMyTimerBlock;
		if (timer__custom_interface_1 == null)
			string_builder__init_info.Append("<warning> no timer found with name \"" + name_group + "\" , ignored\n");

		//检查火炮是否可用
		if(status_cannon == CannonStatus.Invalid)
			//退出
			return;

		//检查火炮初始状态
		if (piston__status_indicator.Status == PistonStatus.Retracted && rotor__status_indicator.IsAttached)
		{//如果 活塞处于紧缩状态 且 转子处于附着状态
			//设置为就绪状态
			this.status_cannon = CannonStatus.Ready;
		}
		else
		{//反之
			//设置为未就绪状态
			this.status_cannon = CannonStatus.NotReady;
			//设置重载命令
			this.command_cannon = CannonCommand.Reload;
		}

		//检查火炮内部功能开关

		//自动检查炮弹分离状态
		flag__check_shell_detach_status =
			locator__cannon_detach_part_grid != null;
		//自动开关焊接器
		flag__auto_toggle_welders_onoff =
			program.flag__auto_toggle_welders_onoff
			&& group_welders != null
			&& list_welders.Count != 0;
		//自动锁定垂直关节
		flag__auto_toggle_vertical_joints_lock =
			program.flag__auto_toggle_vertical_joints_lock 
			&& group__vertical_joints != null 
			&& list__vertical_joints.Count != 0;
		//用户接口定时器0
		flag__auto_trigger_custom_interface_timer_0 =
			program.flag__auto_trigger_custom_interface_timer_0
			&& timer__custom_interface_0 != null;
		//用户接口定时器1
		flag__auto_trigger_custom_interface_timer_1 =
			program.flag__auto_trigger_custom_interface_timer_1
			&& timer__custom_interface_1 != null;

		return;
	}

	public void set_command(CannonCommand cmd)
	{
		this.command_cannon = cmd;
	}

	public PistonStatus get_piston_indicator_status()
	{
		if (piston__status_indicator != null)
			return piston__status_indicator.Status;
		else
			return PistonStatus.Stopped;
	}

	public bool get_rotor_indicator_status()
	{
		if (rotor__status_indicator != null)
			return rotor__status_indicator.IsAttached;
		else
			return false;
	}

	//更新火炮
	public void update()
	{
		//检查火炮可用性
		if (status_cannon == CannonStatus.Invalid)
			return;

		//自动检查
		if (program.flag__auto_check)
		{
			//每一个周期检查一次状态
			//if (accumulation__times_since_last_check >= program.period__auto_check)
			//{
			//	accumulation__times_since_last_check = 1;

			//	//检查完整性
			//	if (!check_cannon_integrality())
			//	{
			//		//火炮不完整
			//		status_cannon = CannonStatus.BrokenDown;
			//	}
			//	else
			//	{
			//		//火炮完整

			//		//检查状态检查之前的状态
			//		if (status_cannon == CannonStatus.BrokenDown)
			//		{
			//			//之前不完整现在完整, 更新状态
			//			status_cannon = CannonStatus.Ready;
			//		}

			//		if (status_cannon == CannonStatus.Ready)
			//		{
			//			//检查状态
			//			if (!check__cannon_status_ready())
			//				status_cannon = CannonStatus.NotReady;
			//		}
			//		else if (status_cannon == CannonStatus.NotReady)
			//		{
			//			//检查状态
			//			if (check__cannon_status_ready())
			//				status_cannon = CannonStatus.Ready;
			//			//尝试重载
			//			if (program.flag__auto_reload)
			//				command_cannon = CannonCommand.Reload;
			//		}
			//	}
			//}
			//else
			//	++accumulation__times_since_last_check;

			if (times__before_next_check == 0)
			{
				times__before_next_check = program.period__auto_check;

				//检查完整性
				if (!check_cannon_integrality())
				{
					//火炮不完整
					status_cannon = CannonStatus.BrokenDown;
				}
				else
				{
					//火炮完整

					//检查状态检查之前的状态
					if (status_cannon == CannonStatus.BrokenDown)
					{
						//之前不完整现在完整, 更新状态
						status_cannon = CannonStatus.Ready;
					}

					if (status_cannon == CannonStatus.Ready)
					{
						//检查状态
						if (!check__cannon_status_ready())
							status_cannon = CannonStatus.NotReady;
					}
					else if (status_cannon == CannonStatus.NotReady)
					{
						//检查状态
						if (check__cannon_status_ready())
							status_cannon = CannonStatus.Ready;
						//尝试重载
						if (program.flag__auto_reload)
							command_cannon = CannonCommand.Reload;
					}
				}

			}
			--times__before_next_check;
		}

		//根据当前状态执行操作
		switch (status_cannon)
		{
		case CannonStatus.NotReady://非就绪状态
			{
				//非就绪状态仅接受重载指令
				if(command_cannon == CannonCommand.Reload)
				{
					//设置为运行状态
					status_cannon = CannonStatus.Loading;
					//运行
					run();
				}
			}
			break;
		case CannonStatus.Ready://就绪状态
			{
				//就绪状态可以接受所有指令

				if (command_cannon == CannonCommand.Fire)
				{
					//开火前检查一次
					if (!check_cannon_integrality() || !check__cannon_status_ready())
					{
						status_cannon = CannonStatus.BrokenDown;
					}
					else
					{
						//更新上一次射击的火炮索引
						group.update_last_fire_index(this.index_cannon);

						//设置为运行状态
						status_cannon = CannonStatus.Loading;
						++program.count_fire;
						run();
					}
				}
				else if (command_cannon == CannonCommand.Reload)
				{
					//设置为运行状态
					status_cannon = CannonStatus.Loading;
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
		case CannonStatus.BrokenDown://故障
		case CannonStatus.Invalid://不可用
			//处于故障状态和不可用状态的火炮不执行任何操作
			break;
		}

		return;
	}

	//内部动作执行函数
	private void run()
	{
		//尝试以下操作
		release();
		detach_begin();
		detach();
		detach_end();
		pistons_extend();
		attach();
		pistons_retract();
		custom_timer_interface_0();
		custom_timer_interface_1();
		done_command();

		//状态计数+1
		++this.count_status;
		return;
	}

	//释放 同时会开启锁定垂直关节
	private void release()
	{
		//释放
		if (count_status == program.delay_release)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				//垂直转子锁定
				if (flag__auto_toggle_vertical_joints_lock)
				{
					foreach (var item in list__vertical_joints)
						item.RotorLock = true;
				}

				//释放
				foreach (var item in list_releasers)
					if (item.IsAttached)
						item.Detach();
			}
		}
	}

	private void detach_begin()
	{
		//分离开始
		if (count_status == program.delay__detach_begin)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				if(program.flag__auto_activate_warhead)
					//重新获取弹头
					update_warheads_list();

				//根据不同的分离模式执行不同操作
				switch (mode_detach)
				{
				case DetachMode.GatlinDestroy://破坏式分离(加特林)
				case DetachMode.GrinderDestroy://破坏式分离(切割机)
					{
						//打开加特林
						//或
						//打开切割机(开始切割)
						foreach (var item in list_detachers)
							item.Enabled = true;
					}
					break;
				case DetachMode.Normal://常规分离(合并块)
					break;
				}
			}
		}
	}

	private void detach()
	{
		//分离
		if (count_status == program.delay_detach)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				//根据不同的分离模式执行不同操作
				switch (mode_detach)
				{
				case DetachMode.GatlinDestroy://破坏式分离(加特林)
					{
						//射击一次
						foreach (var item in list_detachers)
							item.ApplyAction("ShootOnce");
					}
					break;
				case DetachMode.GrinderDestroy://破坏式分离(切割机)
					{
						//关闭加特林
						//或
						//关闭切割机(停止切割)
						foreach (var item in list_detachers)
							item.Enabled = false;
					}
					break;
				case DetachMode.Normal://常规分离(合并块)
					{
						//关闭合并块
						foreach (var item in list_detachers)
							item.Enabled = false;
					}
					break;
				}
			}
		}
	}

	//分离结束 同时会尝试激活弹头
	private void detach_end()
	{
		//分离结束
		if (count_status == program.delay__detach_end)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				//根据不同的分离模式执行不同操作
				switch (mode_detach)
				{
				case DetachMode.GatlinDestroy://破坏式分离(加特林)
				case DetachMode.GrinderDestroy://破坏式分离(切割机)
					break;
				case DetachMode.Normal://常规分离(合并块)
					{
						//关闭合并块
						foreach (var item in list_detachers)
							item.Enabled = true;
					}
					break;
				}

				//激活弹头
				if (flag__auto_activate_warhead)
				{
					if(flag__check_shell_detach_status)
					{
						//检查弹头是否满足激活条件

						//获取网格
						IMyCubeGrid grid_locator = locator__cannon_detach_part_grid.CubeGrid;

						foreach (var item in list_warheads)
						{
							//炮弹所在网格
							IMyCubeGrid grid_shell = item.CubeGrid;
							//直线
							LineD line = new LineD
								(grid_locator.GridIntegerToWorld(locator__cannon_detach_part_grid.Min),
								grid_shell.GridIntegerToWorld(item.Min));
							//检查
							if (grid_locator != grid_shell && line.Length > program.distance__warhead_savety_lock)
							{
								//激活弹头
								if (program.flag__auto_start_warhead_countdown)
								{
									//设置倒计时
									item.DetonationTime = program.number__warhead_countdown_seconds;
									//开始倒计时
									item.StartCountdown();
								}
								item.IsArmed = true;
							}
						}

						//IMyCubeGrid grid_shell = list_warheads[0].CubeGrid;
						////直线
						//LineD line = new LineD
						//	(grid_locator.GridIntegerToWorld(locator__cannon_detach_part_grid.Min),
						//	grid_shell.GridIntegerToWorld(list_warheads[0].Min));
						////检查
						//if (grid_locator != grid_shell && line.Length > program.distance__warheads_savety_lock)
						//{
						//	//激活弹头
						//	if (program.flag__auto_start_warhead_countdown)
						//		foreach (var item in list_warheads)
						//		{
						//			item.IsArmed = true;
						//			item.DetonationTime = program.number__warhead_countdown_seconds;
						//			item.StartCountdown();
						//		}
						//	else
						//		foreach (var item in list_warheads)
						//			item.IsArmed = true;
						//}
					}
					else
						//直接激活
						foreach (var item in list_warheads)
							item.IsArmed = true;
				}
			}
		}
	}

	//活塞伸展 同时会打开焊接器
	private void pistons_extend()
	{
		//活塞伸展 所有指令均需要执行
		if (count_status == program.delay__pistons_extend)
		{
			if (command_cannon != CannonCommand.None)
			{
				//检查状态 活塞不处于已伸展也未正在伸展
				if (piston__status_indicator.Status != PistonStatus.Extended && piston__status_indicator.Status != PistonStatus.Extending)
					foreach (var item in list_pistons)
						item.Extend();

				//打开焊接器
				if (flag__auto_toggle_welders_onoff)
				{
					foreach (var item in list_welders)
						item.Enabled = true;
				}
			}
		}
	}

	//附着 同时会解锁垂直关节
	private void attach()
	{
		//转子附着 所有指令均需要执行
		if (count_status == program.delay_attach)
		{
			if (command_cannon != CannonCommand.None)
			{
				//附着
				foreach (var item in list_releasers)
					//检查是否已经附着
					if (!item.IsAttached)
						item.Attach();

				if (command_cannon == CannonCommand.Fire)
				{
					//垂直转子解锁
					if (flag__auto_toggle_vertical_joints_lock)
					{
						foreach (var item in list__vertical_joints)
							item.RotorLock = false;
					}
				}
			}
		}
	}

	private void pistons_retract()
	{
		//活塞收缩 所有指令均需要执行
		if (count_status == program.dealy__pistons_retract)
		{
			if (command_cannon != CannonCommand.None)
			{
				//检查状态 活塞不处于已收缩也未正在收缩
				if (
					piston__status_indicator.Status != PistonStatus.Retracted
					&& piston__status_indicator.Status != PistonStatus.Retracting
				)
					foreach (var item in list_pistons)
						item.Retract();
			}
		}
	}

	private void custom_timer_interface_0()
	{
		if (!flag__auto_trigger_custom_interface_timer_0)
			return;

		//用户自定义定时器接口0
		if (count_status == program.delay__custom_interface_timer_0)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				//触发用户定时器
				timer__custom_interface_0.Trigger();
			}
		}
	}

	private void custom_timer_interface_1()
	{
		if (!flag__auto_trigger_custom_interface_timer_1)
			return;

		//用户自定义定时器接口1
		if (count_status == program.delay__custom_interface_timer_1)
		{
			if (command_cannon == CannonCommand.Fire)
			{
				//触发用户定时器
				timer__custom_interface_1.Trigger();
			}
		}
	}

	//结束命令 同时会关闭焊接器
	private void done_command()
	{
		//完成指令
		if (count_status == program.delay__done_loading)
		{
			if (command_cannon != CannonCommand.None)
			{
				//关闭焊接器
				if (flag__auto_toggle_welders_onoff)
				{
					foreach (var item in list_welders)
						item.Enabled = false;
				}

				//重置为就绪态
				this.status_cannon = CannonStatus.Ready;
				//重置命令
				this.command_cannon = CannonCommand.None;
				//重置状态计数器(之后会自动+1)
				count_status = 0;
			}
		}
	}

	//更新弹头列表, 弹头在焊接之后需要重新获取
	private void update_warheads_list()
	{
		/********************
		* 获取弹头元件
		********************/
		//拼接字符串, 得到编组名称
		string name_group = this.program.tag__cannon_shell_warheads_group + this.index_cannon;
		//清空旧列表
		list_warheads.Clear();
		//获取弹头元件编组
		group_warheads = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if (group_warheads != null)
			//添加到列表
			group_warheads.GetBlocksOfType<IMyWarhead>(list_warheads);

		//将上一发炮弹的弹头从列表中剔除
		//注意一下, 这里用foreach会出错的, 怎么还犯这种低级错误
		for (var i = 0; i < list_warheads.Count; ++i)
			if (list_warheads[i].CubeGrid == grid__previous_shell)
				list_warheads.RemoveAt(i--);
		//更新网格记录
		if (list_warheads.Count > 0)
			grid__previous_shell = list_warheads[0].CubeGrid;
		else
			grid__previous_shell = null;

		this.flag__auto_activate_warhead =
			group_warheads != null
			&& list_warheads.Count != 0;
	}

	//检查火炮在在物理上是否真的处于就绪状态
	private bool check__cannon_status_ready()
	{
		//检查活塞状态指示器
		//如果 活塞不是已收缩或者收缩中
		if (piston__status_indicator.Status != PistonStatus.Retracted && piston__status_indicator.Status != PistonStatus.Retracting)
		{
			//将火炮状态设为故障
			//this.status_cannon = CannonStatus.BrokenDown;
			return false;
		}
		else//活塞已经收缩, 或正在收缩中
		{
			//检查转子是否成功附着
			if (!rotor__status_indicator.IsAttached)
				//将火炮状态设为故障
				//this.status_cannon = CannonStatus.BrokenDown;
				return false;
		}
		return true;
	}

	//检查火炮完整性
	private bool check_cannon_integrality()
	{
		//结果
		bool res = true;

		//检查活塞元件完整性
		foreach(var item in list_pistons)
		{
			if (!item.IsWorking)
			{
				res = false;
				break;
			}
		}

		//检查释放元件完整性
		foreach (var item in list_releasers)
		{
			if (!item.IsWorking)
			{
				res = false;
				break;
			}
				
		}

		return res;
	}

	//获取火炮信息
	public string get_cannon_info()
	{
		return "<cannon> -------------------- No." + this.index_cannon
			+ "\n<count_status> " + this.count_status
			+ "\n<status> " + status_cannon.ToString()
			+ "\n<command> " + command_cannon.ToString()
			+ "\n<rotor_attached> " + this.get_rotor_indicator_status().ToString()
			+ "\n<status_pistons> " + this.get_piston_indicator_status().ToString()
			+ "\n<count_pistons> " + this.list_pistons.Count
			+ "\n<flag_AAWH> " + this.flag__auto_activate_warhead
			+ "\n<flag_ATWOO> " + this.flag__auto_toggle_welders_onoff
			+ "\n<flag_ATVJL> " + this.flag__auto_toggle_vertical_joints_lock
			+ "\n<flag_ATCIT0> " + this.flag__auto_trigger_custom_interface_timer_0
			+ "\n<flag_ATCIT1> " + this.flag__auto_trigger_custom_interface_timer_1
			+ "\n\n <init_info> \n" + string_builder__init_info.ToString();
	}

	public string get_cannon_LCD_info()
	{
		return "--------------------<cannon>--------------------"
			+ "\n<cannon> No." + this.index_cannon
			+ "\n<count_status> " + this.count_status
			+ "\n<status> " + status_cannon.ToString()
			+ "\n<command> " + command_cannon.ToString()
			+ "\n<rotor_attached> " + this.get_rotor_indicator_status().ToString()
			+ "\n<status_pistons> " + this.get_piston_indicator_status().ToString()
			+ "\n<count_pistons> " + this.list_pistons.Count
			+ "\n<flag_AAWH> " + this.flag__auto_activate_warhead
			+ "\n<flag_ATWOO> " + this.flag__auto_toggle_welders_onoff
			+ "\n<flag_ATVJL> " + this.flag__auto_toggle_vertical_joints_lock
			+ "\n<flag_ATCIT0> " + this.flag__auto_trigger_custom_interface_timer_0
			+ "\n<flag_ATCIT1> " + this.flag__auto_trigger_custom_interface_timer_1;
	}

	//获取火炮简易信息
	public string get_cannon_simple_info()
	{
		return
			"<cannon> No." + this.index_cannon
			+ " " + this.status_cannon.ToString()
			+ " " + this.command_cannon.ToString()
			+ " " + this.get_piston_indicator_status().ToString();
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
	public CannonGroup(Program _program, int _index_group)
	{
		program = _program;
		index_group = _index_group;

		/********************
		* 获取射击指示器元件
		********************/
		//拼接字符串, 得到编组名称
		string name_group = this.program.tag__cannon_group_fire_indicators_group + this.index_group;
		//获取射击指示器元件编组
		group__fire_indicators = program.GridTerminalSystem.GetBlockGroupWithName(name_group);
		//检查
		if (group__fire_indicators == null)
		{
			string_builder__init_info.Append("<warning> no group found with name \"" + name_group + "\" , ignored\n");
		}
		else
		{
			//添加到列表
			group__fire_indicators.GetBlocksOfType<IMyUserControllableGun>(list__fire_indicators);
			//检查
			if (list__fire_indicators.Count == 0)
			{
				string_builder__init_info.Append("<warning> no any weapon found in group \"" + name_group + "\"\n");
			}
		}

		mode_fire = program.mode_fire__intra_group;
		return;
	}

	public void set_group_fire_mode(FireMode mode)
	{
		this.mode_fire = mode;
	}

	//添加火炮对象到编组
	public void add_cannon(PistonCannon cannon)
	{
		//设置起始索引
		if (list_cannons.Count == 0)
		{
			index__cannon_begin = cannon.index_cannon;
			index_begin = index__cannon_begin - program.index__cannon_begin;
		}
		//添加到列表
		list_cannons.Add(cannon);
		index__cannon_last_fire = list_cannons.Count - 1;
		//更新末尾索引
		index__cannon_end = list_cannons[list_cannons.Count - 1].index_cannon;
		index_end = index__cannon_end - program.index__cannon_begin;
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
	public int fire()
	{
		if (program.mode_script == ScriptMode.WeaponSync && !flag__fire_indicators_activated)
			return 0;

		int count = 0;
		switch (mode_fire)
		{
		case FireMode.Salvo://齐射
			count = fire_salvo();
			break;
		case FireMode.Round://轮射
			count = fire_round();
			break;
		}
		//更新上一次射击的编组
		if (count != 0)
			program.index__group_last_fire = index_group;
		return count;
	}

	//齐射 
	//返回有几门火炮进行射击
	private int fire_salvo()
	{
		int count = 0;

		//逐个遍历寻找所有处于 Ready 状态的火炮
		foreach (var item in list_cannons)
		{
			//检查状态
			if (item.status_cannon == PistonCannon.CannonStatus.Ready)
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
		int index__last_next = index__cannon_last_fire + 1;

		for (i = index__last_next; i < list_cannons.Count; ++i)
		{
			//检查状态
			if (list_cannons[i].status_cannon == PistonCannon.CannonStatus.Ready)
			{
				//设置射击命令
				list_cannons[i].set_command(PistonCannon.CannonCommand.Fire);
				return 1;
			}
		}

		//仍然没有找到
		if (i == list_cannons.Count)
		{
			//从开头重新查找到上次开炮的位置
			for (i = 0; i < index__last_next; ++i)
			{
				//检查状态
				if (list_cannons[i].status_cannon == PistonCannon.CannonStatus.Ready)
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
		flag__fire_indicators_activated = false;
		foreach (var item in list__fire_indicators)
			if (item.IsShooting)
			{
				flag__fire_indicators_activated = true;
				break;
			}
		return flag__fire_indicators_activated;
	}

	//更新上一次射击火炮索引
	public void update_last_fire_index(int index)
	{
		this.index__cannon_last_fire = index - index__cannon_begin;
	}

	//返回就绪的火炮数量
	private int count_ready_cannon()
	{
		int count = 0;
		foreach (var item in list_cannons)
			if (item.status_cannon == PistonCannon.CannonStatus.Ready)
				++count;
		return count;
	}

	//获取火炮信息
	public string get_group_info()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(
			"\n<group> ------------------------------ No." + this.index_group
			+ "\n<mode_fire> " + this.mode_fire
			+ "\n\n");
		foreach (var item in list_cannons)
			sb.Append(item.get_cannon_info() + "\n");
		sb.Append(string_builder__init_info);
		return sb.ToString();
	}

	public string get_group_LCD_info()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(
			"--------------------<group>--------------------"
			+ "\n<group> No." + this.index_group
			+ "\n<mode_fire> " + this.mode_fire
			+ "\n----------<cannons>----------\n");
		foreach (var item in list_cannons)
			sb.Append(item.get_cannon_simple_info() + "\n");
		return sb.ToString();
	}

	internal void toggle_group_fire_mode()
	{
		if (mode_fire == FireMode.Round)
			mode_fire = FireMode.Salvo;
		else
			mode_fire = FireMode.Round;
	}
}

#endregion

#region 配置通用

/***************************************************************************************************
* 类 自定义数据配置
* 自定义数据配置(下简称CD配置)使用目标方块的自定义数据来进行脚本配置
* 这部分是参考MART中的CD配置通用代码重写并改进的, 增强了一定泛用性
* 
* ============================== 使用方法 ==============================
* 
* [0] 构造一个本类的对象(实例), 用于管理特定方块的CD配置, 构造函数传递目标方块对象引用即可
* [1] 使用以下成员函数来添加配置项:
*     add_config_item(nameof(VARIABLE), () => VARIABLE, (x) => { VARIABLE = (TYPE)x; });
*     其中 VARIABLE 是变量, TYPE是它的类型
*     
* [2] 执行 init_config() 函数来初始化配置, 这一步会解析目标方块的自定义数据并读取数据,
*     如果解析没有遇到错误就会覆盖写入, 如果遇到非法配置内容, 会读取合法的数据但不会覆盖写入
*     此时对象的 flag__config_error_in_CD 成员变量会被设为true, 
*     且错误信息会被输出到 string_builder__error_info 成员变量中
* 
***************************************************************************************************/
public class CustomDataConfig
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
			this.get = _getter;
			this.set = _setter;
		}
	}

	//终端方块 CD配置的目标方块
	public IMyTerminalBlock block_target { get; private set; } = null;

	//字典 配置项字典
	Dictionary<string, ConfigItemReference> dict__config_items = new Dictionary<string, ConfigItemReference>();

	//字符串构建器 用户数据写入
	public StringBuilder string_builder__CD_write { get; private set; } = new StringBuilder();

	public StringBuilder string_builder__error_info { get; private set; } = new StringBuilder();

	//标记 CD中发现错误(存在错误时不会覆盖写入)
	public bool flag__config_error_in_CD { get; private set; } = false;

	//构造函数
	public CustomDataConfig(IMyTerminalBlock _block_target)
	{
		this.block_target = _block_target;
	}

	//初始化配置
	public void init_config()
	{
		//解析自定义数据
		parse_custom_data();

		//检查CD配置是否存在错误
		if (!flag__config_error_in_CD)
			//写入自定义数据
			write_to_block_custom_data();
	}

	//添加配置项
	public bool add_config_item(string name_config_item, Func<object> getter, Action<object> setter)
	{
		//检查是否已经包含此配置项目
		if (dict__config_items.ContainsKey(name_config_item))
			return false;
		//添加到字典
		dict__config_items.Add(name_config_item, new ConfigItemReference(getter, setter));
		return true;
	}

	//更新配置项的值
	public bool update_config_item_value(string name__config_item, object value__config_item, bool flag_rewrite = true)
	{
		if (!dict__config_items.ContainsKey(name__config_item))
			return false;

		dict__config_items[name__config_item].set(value__config_item);
		//重新写入
		if (flag_rewrite)
			write_to_block_custom_data();
		return true;
	}

	//解析方块自定义数据
	//当出现
	private void parse_custom_data()
	{
		//以换行符拆分
		string[] array_lines = block_target.CustomData.Split('\n');
		int count = 0;

		foreach (var line in array_lines)
		{
			++count;
			if (line.Length == 0)
				continue;//跳过空行

			//以等号拆分
			string[] pair = line.Split('=');
			//检查
			if (pair.Length != 2)
			{
				string_builder__error_info.Append($"<error> \"{line}\"(at line {count}) is not a legal config item");
				flag__config_error_in_CD = true;
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
				flag__config_error_in_CD = true;
			}
		}
	}

	//写入方块自定义数据
	private void write_to_block_custom_data()
	{
		string_builder__CD_write.Clear();
		foreach (var pair in dict__config_items)
			string_builder__CD_write.Append($"{pair.Key} = {pair.Value.get()}\n");
		block_target.CustomData = string_builder__CD_write.ToString();
	}

	//解析字符串值
	private bool parse_string(string str_value, ref object variable)
	{
		if (variable is bool)
		{
			bool value_parsed;
			if (bool.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		else if (variable is float)
		{
			float value_parsed;
			if (float.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		else if (variable is double)
		{
			double value_parsed;
			if (double.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		else if (variable is int)
		{
			int value_parsed;
			if (int.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		else if (variable is Vector3D)
		{
			Vector3D value_parsed;
			if (Vector3D.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		else if (variable is string)
		{
			variable = str_value;
			return true;
		}
		else if (variable is ScriptMode)
		{
			ScriptMode value_parsed;
			if (ScriptMode.TryParse(str_value, out value_parsed))
			{
				variable = value_parsed;
				return true;
			}
		}
		return false;
	}

}


#endregion
