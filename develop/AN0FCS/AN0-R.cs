﻿/***************************************************************************************************
* 
* ### AN0FCS's Not Only Fire Control System ###
* ### AN0FCS | "匿名者" 火控系统脚本 ------ ###
* ### Version 0.0.0 | by SiriusZ-BOTTLE --- ###
* ### STPC旗下SCPT工作室开发, 欢迎加入STPC  ###
* ### STPC主群群号:320461590 我们欢迎新朋友 ###
* 
* 
***************************************************************************************************/


#define DEVELOP

#if DEVELOP
//用于IDE开发的 using 声明

// 系统库
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
// 游戏库
using VRage;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace AN0_RADAR_DEV
{
    class Program : MyGridProgram
    {
#endif

        #region 脚本字段

        // 字符串 脚本版本号
        readonly string string__script_version = "AN0-RADAR V0.0.0-BETA";

        // 数组 运行时字符显示
        readonly string[] string_array__runtime_patterns = new string[]
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
        // 无目标信息的字符串
        readonly string string__no_target_info = "<target> no info";

        // 名称 脚本核心编组
        string name__script_core_group = "AN0-RADAR";
        // 字符串 本地广播信道
        string string__local_broadcast_channel = "AN0-R-LOCAL-BROADCASR-CHANNEL-#0";
        // 字符串 内部通信信道
        string string__internal_communication_channel = "AN0-R-INTERNAL-COMMUNICATION-CHANNEL-#0";
        // 字符串 扫描坐标信道
        string string__scanning_coordinate_channel = "AN0-R-SCANNING-COORDINATE-CHANNEL-#0";

        // 标记 是否 获取自动炮塔目标
        bool flag__obtain_auto_turret_targets = true;
        // 标记 是否 启用无限投射
        bool flag__enable_unlimited_raycast = false;
        // 标记 是否 当锁定时注册额外对象
        bool flag__register_extra_targets_when_locking = true;
        // 标记 是否 当离线 (无用户操控) 时继续锁定目标
        bool flag__keep_targets_locking_while_offline = true;
        // 标记 是否 自动对摄像头进行分组
        bool flag__group_cameras_automatically = false;

        // 与通信有关的设置

        // 标记 是否 当离线 (无用户操控) 时广播目标 (本地)
        bool flag__enable_targets_localcasting_while_offline = true;
        // 标记 是否 当离线 (无用户操控) 时共享目标 (网络)
        bool flag__enable_targets_broadcasting_while_offline = true;
        // 标记 是否 当离线 (无用户操控) 时接收目标 (网络)
        bool flag__enable_targets_receiving_while_offline = true;
        // 标记 是否 启用实例间通信
        bool flag__enable_inter_instance_communication = true;
        // 标记 是否 接收扫描坐标
        bool flag__receiving_scanning_coordinate = true;

        // 全局目标过滤器 (只针对射线投射目标, 不过滤自动炮塔目标)

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
        long period__auto_check = 60;
        //周期 更新输出
        long period__update_output = 30;
        //周期 更新自动炮塔对象
        long period__update_targets_of_auto_turrets = 1;
        // 延迟 当丢失目标之后依旧尝试锁定的时间
        long delay__after_missing = 120;

        // 距离 最小距离 (低于此距离的目标不会被摄像头锁定)
        double distance__min = 10;
        // 距离 最大距离 (锁定时的最大的投射距离)
        double distance__max = 5000;
        // 距离 扫描距离
        double distance__scan = 2000;

        // 倍率 投射倍率
        double ratio__raycast = 1.01;
        // 常数 投射常数
        double constant__raycast = 10;

        // 注: 摄像头消耗能量 = 原始投射距离 * ratio__raycast + constant__raycast


        // 次数 期望维持的能量对应的投射次数
        // 意味着脚本会始终尝试将能量维持到能满足此变量对应的投射次数所需的能量水平
        // 此值无需过大, 但也避免过小, 以提供一定的容错性, 应对可能突然增长的能量消耗需求
        double number__raycast_of_expectation_energe = 100;

        // 最低频率, 代表即使在最坏的情况下脚本也以该频率尝试投射
        double frequency__min = 0.25;
        // 对数函数的底数
        double base__log = Math.E;


        // 以上是脚本配置字段


        //列表 显示单元
        List<DisplayUnit> list__display_units = new List<DisplayUnit>();

        //标记 用户自定义目标被锁定
        bool flag__custom_tgt_locked;
        // 标记 脚本正在扫描中
        bool flag__scanning;
        // 标记 驾驶舱正在操控中
        bool flag__under_control;

        // 计数 脚本运行命令次数
        long count__cmd_run = 0;
        // 技术 接受的消息数
        long count__message_received = 0;
        // 计数 更新
        long count__update = 0;
        // 索引 当前字符图案
        long index__crt_char_pattern = 0;
        // 次数 距离下一次 字符图案更新 (times before next)
        long time__before_next_char_pattern_update = 0;
        // 次数 距离下一次 更新输出
        long time__before_next_output_update = 0;

        // 周期 扫描
        double period__scanning = 0;
        // 周期 锁定
        double period__locking = 0;

        // 次数 距离下一次 扫描
        double time__before_next_scanning = 0;
        // 次数 距离下一次 锁定
        double time__before_next_locking = 0;

        // 时间戳(脚本全局, ms)
        long timestamp = 0;
        // 时间段
        long time_span;

        // 计数 对扫描有效的摄像头 (注意这是一个浮点数, 相当于当前有效性之和)
        double count__effective_camera_for_scanning;
        // 计数 对锁定有效的摄像头 (注意这是一个浮点数, 相当于当前有效性之和)
        double count__effective_camera_for_locking;
        // 总射程 对扫描有效的
        double total_range__effective_for_scanning;
        // 总射程 对锁定有效的
        double total_range__effective_for_locking;
        // 射程变化量 (持续取平滑值)
        double variation__range;
        // 摄像头射程总量(每一帧更新)
        double sum__range;

        // 索引 上一次用于扫描的摄像头的索引
        int index__last_camera_for_scanning = -1;
        // 索引 上一次锁定用摄像头的索引
        int index__last_camera_for_locking = -1;

        // 优先级 上一次用于扫描的摄像头的优先级
        double priority__of_last_camera_for_scanning = 0;
        // 优先级 上一次用于锁定的摄像头的优先级
        double priority__of_last_camera_for_locking = 0;

        PidCore pid_core__scanning = new PidCore
        (
            0.03, // P
            0.02, // I
            0.09, // D
            1.0, // 衰减系数
            1.0, // 最大倍率
            5, // 输出上限
            -5, // 输出下限
            100, // 积分分离阈值
            100 // 积分上限
        );

        //当前位置, 该值被视作脚本所在的实体的核心位置
        Vector3D position;
        //向量3D 脚本 (扫描) 目标位置
        Vector3D vector__scanning_coordinate = new Vector3D();

        // 当前操控中的控制器
        IMyShipController controller__main = null;

        // 字典 射线投射锁定目标
        Dictionary<long, Target> dict__targets_of_raycast = new Dictionary<long, Target>();
        // 字典 自动炮台目标
        Dictionary<long, Target> dict__targets_of_auto_turrets = new Dictionary<long, Target>();
        // 字典 从其它实例中获取的目标
        Dictionary<long, Target> dict__targets_from_other_instance = new Dictionary<long, Target>();

        // 字典 短时间内的摄像头有效性评价 用于扫描 (1.0 为最大值, 0 为最小值)
        Dictionary<IMyCameraBlock, CameraEvaluationInformation> dict__camera_effectiveness_at_the_time__scanning = new Dictionary<IMyCameraBlock, CameraEvaluationInformation>();
        // 字典 短时间内摄像头的有效性评价 用于锁定 (值域同上)
        Dictionary<IMyCameraBlock, CameraEvaluationInformation> dict__camera_effectiveness_at_the_time__locking = new Dictionary<IMyCameraBlock, CameraEvaluationInformation>();

        //列表 射线投射锁定目标
        List<Target> list__targets_of_raycast = new List<Target>();
        //列表 自动炮台目标
        List<Target> list__targets_of_auto_turret = new List<Target>();
        // 列表 其它实例的目标
        List<Target> list__targets_from_other_instance = new List<Target>();
        // 列表 全部目标 (包含上面三个列表中的全部对象)
        List<Target> list__all_targets = new List<Target>();

        //字符串构建器 标题信息
        StringBuilder string_builder__title = new StringBuilder();
        //字符串构建器 脚本信息
        StringBuilder string_builder__script_info = new StringBuilder();

        //字符串构建器 目标信息
        StringBuilder string_builder__targets = new StringBuilder();
        //字符串构建器 测试信息
        StringBuilder string_builder__test_info = new StringBuilder();

        //对象管理器
        ObjectManager object_manager__script;

        // 监听器 实例间的目标数据通信
        IMyBroadcastListener listener__inter_instance_communication;
        // 监听器 扫描坐标
        IMyBroadcastListener listener__scanning_coordinate;

        //脚本配置
        DataConfig data_config__script;
        //脚本配置集合
        DataConfigSet data_config_set__script;


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
        void Main(string string_arg, UpdateType type_update)
        {
            switch (type_update)
            {
                case UpdateType.Terminal:
                case UpdateType.Trigger:
                case UpdateType.Script:
                case UpdateType.IGC:
                {
                    run_command(string_arg);
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
            //try
            {
                // 更新摄像头数据
                update_camera_data();

                // 更新时间戳
                update_timestamp();

                // 更新自动炮塔目标
                update_auto_turret_targets();

                // 更新控制器状态
                update_controllers_status();

                // 更新通信数据
                update_communication_data();

                //当前位置设为编程块所在位置
                position = Me.GetPosition();

                // 尝试锁定目标
                lock_targets();

                // 更新目标数据
                update_targets_info();

                if (flag__scanning)
                    scan();// 尝试扫描

                // 更新对象列表
                update_targets_info();
            }
            //catch (Exception e)
            //{
            //    //Me.CustomData = e.ToString();
            //    throw e;
            //}

            //信息显示
            display_info();

            //更新次数+1
            ++count__update;
        }

        // 更新通信数据 (发送和接收消息)
        void update_communication_data()
        {
            // 先接收数据再发送
            receive_data();
            cast_data();

            // 若不启用通信也不会接收信息
            while (listener__inter_instance_communication.HasPendingMessage)
            {
                // 接受消息
                MyIGCMessage message = listener__inter_instance_communication.AcceptMessage();


                // 更新计数
                ++count__message_received;
            }

            if (true)
            {

            }
        }

        // 转为元组的列表 (用以传输)
        static List<MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>>>
            enum_to_tuple_list(IEnumerable<Target> collection)
        {
            var list = new List<MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>>>();
            // 逐个转换
            foreach (var item in collection)
                list.Add(item.to_exchange_info());
            return list;
        }

        // 接收信息
        void receive_data()
        {

        }

        // 广播信息
        void cast_data()
        {
            // 检查是否启用了脚本实例间通信
            if (flag__enable_inter_instance_communication)
            {
                // 检查用户是否在线, 或开启了离线时共享目标
                if (flag__enable_targets_localcasting_while_offline)
                {
                    // 将所有信息进行本地广播

                    MyTuple<long, MyTuple<long>> tuple = new MyTuple<long, MyTuple<long>>();
                    //list__tuples;

                    //var array = ImmutableArray.CreateRange
                    //    <MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>>>
                    //    ();

                }

                if (flag__enable_targets_broadcasting_while_offline)
                {

                }




                //MyDetectedEntityInfo entity = new MyDetectedEntityInfo();

                //IGC.SendBroadcastMessage(string__internal_communication_channel, entity, TransmissionDistance.TransmissionDistanceMax);


            }
        }

        // 更新目标信息 (维护列表)
        void update_targets_info()
        {
            // 重建列表对象
            list__targets_of_raycast = new List<Target>(dict__targets_of_raycast.Values);
            list__targets_of_auto_turret = new List<Target>(dict__targets_of_auto_turrets.Values);
            list__targets_from_other_instance = new List<Target>(dict__targets_from_other_instance.Values);

            list__all_targets.Clear();

            var dict__all_targets = new Dictionary<long, Target>(dict__targets_of_raycast);
            // 将剩下两个列表的对象也加入字典
            foreach (var item in list__targets_of_auto_turret)
                dict__all_targets.Add(item.id, item);
            foreach (var item in list__targets_from_other_instance)
                dict__all_targets.Add(item.id, item);

            //list__all_targets.AddRange();
        }

        // 更新摄像头数据
        void update_camera_data()
        {
            //var tmp = sum__range;
            //sum__range = 0;
            //// 更新当前帧摄像头总射程
            //foreach (var i in object_manager__script.list_camera)
            //    sum__range += i.AvailableScanRange;
            //// 更新射程变化量 (平滑)
            //variation__range = TK.cal_smooth_avg(variation__range, sum__range - tmp);

            if (flag__scanning)
            {
                total_range__effective_for_scanning = 0;
                count__effective_camera_for_scanning = 0;
                foreach (var pair in dict__camera_effectiveness_at_the_time__scanning)
                {
                    double effectiveness;
                    total_range__effective_for_scanning += pair.Value.update(out effectiveness);
                    count__effective_camera_for_scanning += effectiveness;
                }
            }

        }

        // 更新控制器状态
        void update_controllers_status()
        {
            controller__main = null;

            // 找到第一个处于控制的控制器
            foreach (var i in object_manager__script.list_controller)
                if (i.IsUnderControl)
                {
                    controller__main = i;
                    break;
                }
            flag__under_control = controller__main != null;
        }

        // 更新自动炮塔对象
        void update_auto_turret_targets()
        {
            HashSet<long> set__target_id = new HashSet<long>();
            // 获取当前帧自动炮塔的目标集合 (新的目标创建, 已有的目标更新)
            foreach (var turret in object_manager__script.list__auto_turret)
                if (turret.HasTarget)
                {
                    var entity = turret.GetTargetedEntity();
                    set__target_id.Add(entity.EntityId);
                    if (dict__targets_of_auto_turrets.ContainsKey(entity.EntityId))
                        // 存在目标则更新
                        dict__targets_of_auto_turrets[entity.EntityId].set(timestamp, entity, position);
                    else
                        // 不存在则新建
                        dict__targets_of_auto_turrets[entity.EntityId] = new Target(timestamp, entity, position);
                }
            // 删除已经不存在的目标
            foreach (var kvp in dict__targets_of_auto_turrets)
                if (!set__target_id.Contains(kvp.Key))
                    dict__targets_of_auto_turrets.Remove(kvp.Key);
        }

        // 更新时间戳
        void update_timestamp()
            => timestamp += time_span = Runtime.TimeSinceLastRun.Milliseconds;

        //执行指令
        void run_command(string string_arg)
        {
            var cmd = TK.split_string(string_arg);
            ++count__cmd_run;//更新计数
            if (cmd.Length == 0)
            {
                // 没有写参数
                toggle_scanning_status();
            }
            else if (cmd.Length == 1)
            {
                // 一个参数
                switch (cmd[0])
                {
                    case "":
                    case "toggle_scanning_status":
                    {
                        // 切换扫描状态
                        toggle_scanning_status();
                        break;
                    }
                    case "stop_locking":
                    {
                        // 解除锁定
                        stop_locking();
                        break;
                    }
                    case "terminate":
                    {
                        break;
                    }
                }
            }
            else
            {
                // 两个参数
                int index_group = 0;
                int.TryParse(cmd[1], out index_group);
                switch (cmd[0])//检查命令
                {
                    case "scan":
                        break;
                }
            }
        }

        // 切换扫描状态
        void toggle_scanning_status()
        {
            flag__scanning = !flag__scanning;
        }

        // 停止锁定
        void stop_locking()
        {
            dict__targets_of_raycast.Clear();
        }

        // 扫描
        void scan()
        {
            // 计算当前处于控制中的驾驶舱的正前方的坐标

            // 不处于控制中则退出
            if (!flag__under_control)
                return;
            Vector3D position__controller = controller__main.GetPosition();
            Vector3D vector__target = position__controller + controller__main.WorldMatrix.Forward * distance__scan;

            RaycastInfo info = null;

            bool flag__success = false;

            if (time__before_next_scanning <= 0)
            {
                // 朝坐标进行投射
                flag__success = cast_at_coordinate(out info, vector__target, dict__camera_effectiveness_at_the_time__scanning);

                // 设置下一次扫描的冷却时间
                if (flag__enable_unlimited_raycast)
                    // 无限投射则每一帧都进行投射
                    time__before_next_scanning += (period__scanning = 1.0f);
                else
                    // 根据当前能量计算投射频率
                    time__before_next_scanning += (period__scanning = TK.calculate_period
                        (total_range__effective_for_scanning, // 当前有效能量
                        number__raycast_of_expectation_energe * distance__scan, // 期望能量
                        count__effective_camera_for_scanning * 2000.0 / 60.0, // 当前有效增量
                        frequency__min, // 最低频率
                        ratio__raycast * distance__scan + constant__raycast, // 
                        pid_core__scanning)); // PID 控制核心

                string_builder__test_info.Append($"PID output: {pid_core__scanning.output__last.ToString("0.000f")}\n");
            }
            --time__before_next_scanning;


            // 存在值, 并且距离大于下限
            if (flag__success && info.entity_info.HasValue && (position__controller - info.entity_info.Value.Position).Length() > distance__min)
                register_target_in_dict(info.entity_info.Value);
        }

        // 朝指定坐标投射
        // 返回是否成功, 成功意味着不光进行了投射, 投射到了目标, 还意味着通过了筛选器
        // 返回与投射相关的信息, 包括使用了什么摄像头, 投射距离等等, 若探测到目标, 即使没有通过筛选器依旧会返回
        bool cast_at_coordinate(out RaycastInfo _raycast_info, Vector3D _vector_coordinate, Dictionary<IMyCameraBlock, CameraEvaluationInformation> _dict__camera_effectiveness)
        {
            _raycast_info = null;

            // 两次遍历方式, 从上一次选中的目标摄像头的位置向后遍历, 若未发现满足条件的摄像头则从头遍历

            // 遍历全部摄像头 (从上一次的位置开始)
            if (index__last_camera_for_scanning < object_manager__script.list_camera.Count)
                _raycast_info = access_all_the_cameras
                    (ref index__last_camera_for_scanning, ref priority__of_last_camera_for_scanning, index__last_camera_for_scanning + 1, _vector_coordinate);

            if (_raycast_info == null)
                _raycast_info = access_all_the_cameras
                    (ref index__last_camera_for_scanning, ref priority__of_last_camera_for_scanning, 0, _vector_coordinate);

            // 扫描到对象
            if (_raycast_info != null && _raycast_info.entity_info.HasValue && !_raycast_info.entity_info.Value.IsEmpty())
            {
                // 设置返回的信息

                bool flag__exit = false;
                switch (_raycast_info.entity_info.Value.Type) // 类型过滤器, 若不通过过滤器本函数将会返回 false
                {
                    case MyDetectedEntityType.CharacterHuman://人类角色
                    case MyDetectedEntityType.CharacterOther://非人角色
                        if (flag__ignore_players) flag__exit = true; break;
                    case MyDetectedEntityType.Missile://小型网格
                        if (flag__ignore_rockets) flag__exit = true; break;
                    case MyDetectedEntityType.Meteor://小型网格
                        if (flag__ignore_meteors) flag__exit = true; break;
                    case MyDetectedEntityType.SmallGrid://小型网格
                        if (flag__ignore_small_grids) flag__exit = true; break;
                    case MyDetectedEntityType.LargeGrid://大型网格
                        if (flag__ignore_large_grids) flag__exit = true; break;
                    case MyDetectedEntityType.FloatingObject://漂浮物
                    case MyDetectedEntityType.Asteroid://小行星
                    case MyDetectedEntityType.Planet://行星
                    case MyDetectedEntityType.Unknown://未知
                    case MyDetectedEntityType.None://空
                        flag__exit = true; break;//被忽略的类型
                }
                switch (_raycast_info.entity_info.Value.Relationship)//关系过滤器
                {
                    case MyRelationsBetweenPlayerAndBlock.Friends://友方
                    case MyRelationsBetweenPlayerAndBlock.FactionShare://阵营共享
                        if (flag__ignore_the_friendly) flag__exit = true; break;
                    case MyRelationsBetweenPlayerAndBlock.Neutral://中立方
                    case MyRelationsBetweenPlayerAndBlock.NoOwnership://无归属
                        if (flag__ignore_the_neutral) flag__exit = true; break;
                    case MyRelationsBetweenPlayerAndBlock.Enemies://敌对方
                        if (flag__ignore_the_enemy) flag__exit = true; break;
                }

                if (flag__exit)
                    // 未通过筛选器, 类型或关系不满足需求
                    return false;
                else
                    return true;
            }
            else
            {
                // 使用默认构造函数, 将会构造一个不具有有效性的 RaycastInfo 实例
                _raycast_info = new RaycastInfo();
                // 没有查询到目标
                return false;
            }
        }

        // 遍历所有的摄像头, 获取投射信息
        // [in] _index__last 上一次的索引
        // [in] _priority__last 上一次的优先级
        // [in] _priority__last 上一次投射的优先级
        // [return] 投射相关信息, 若没有摄像头满足优先级条件, 则会返回 null
        RaycastInfo access_all_the_cameras
            (ref int _index__last, ref double _priority__last, int _index__begin, Vector3D _vector_coordinate)
        {
            Echo($"_index__begin : {_index__begin}");

            RaycastInfo raycast_info__result = null;

            // 遍历全部摄像头, 从上一次的位置遍历
            for (int index = _index__begin; index < object_manager__script.list_camera.Count; ++index)
            {
                IMyCameraBlock camera = object_manager__script.list_camera[index];

                var info = dict__camera_effectiveness_at_the_time__scanning[camera];

                // 检查优先级, 必须满足当前选中的摄像头的优先级大于上一次投射的摄像头的优先级的一定比例
                // 该检查用于过滤, 避免进入 try_raycast() 函数, 降低脚本总体开销
                if (info.priority >= 0.5 * _priority__last)
                {
                    // 尝试进行投射, 返回投射信息
                    raycast_info__result = try_raycast(_vector_coordinate, info);
                }

                if (raycast_info__result != null && raycast_info__result.flag__did_raycasted)
                {
                    if (raycast_info__result.entity_info != null)
                    {
                        if (object_manager__script.set__self_id.Contains(raycast_info__result.entity_info.Value.EntityId))
                        {
                            // 扫描到自身网格, 视为失败投射, 跳过
                            raycast_info__result = null;
                            continue;
                        }
                    }

                    // 扫描成功, 结束 (无论是否扫描到对象)

                    // 记录当前信息
                    _index__last = index;
                    _priority__last = info.priority;
                    break;
                }
                else
                {
                    // 无法投射
                    continue;
                }
            }
            return raycast_info__result;
        }

        // 尝试进行投射, 返回投射信息, 但摄像头并非一定发出投射
        // [in] _vector_coordinate 投射坐标
        // [in] _info 摄像头评估信息
        // [return] 与投射相关的信息, 一定会返回一个实例, 返回值不会为 null
        // 注: 若投射成功但没有发现目标, 返回的实例的 entity_info 成员将是 null
        RaycastInfo try_raycast(Vector3D _vector_coordinate, CameraEvaluationInformation _info)
        {
            MyDetectedEntityInfo? entity_info = null;
            IMyCameraBlock camera = _info.camera;

            // 投射朝向向量
            var orientation = _vector_coordinate - camera.WorldMatrix.Translation;
            // 偏航, 俯仰 (用于摄像头投射)
            float yaw, pitch;

            // 全局朝向转 YP
            TK.global_to_yaw_pitch(out yaw, out pitch, camera.WorldMatrix, orientation);
            // 使用固定的倍率 * 投射长度, 提供容错性
            double distance = ratio__raycast * orientation.Length() + constant__raycast;
            // 计算摄像头 F 向量与投射向量的夹角
            double angle__crt = 180 / Math.PI * TK.calculate_angle(camera.WorldMatrix.Forward, orientation);

            // 检查投射距离是否超过上限, 检查摄像头能量是否充足, 检查是否处于射界内
            bool flag = distance > distance__max
                || distance < camera.AvailableScanRange
                || Math.Abs(yaw) > camera.RaycastConeLimit
                || Math.Abs(pitch) > camera.RaycastConeLimit;

            // 更新评估信息
            _info.update_after_cast(angle__crt, flag);
            if (flag)
            {
                // 施放投射
                entity_info = camera.Raycast(distance, pitch, yaw);
                // 检查是否扫描到物体
                if (entity_info.Value.IsEmpty())
                    entity_info = null;
            }
            return new RaycastInfo(camera, distance, entity_info, angle__crt, flag);
        }

        // 将目标信息在字典中注册
        void register_target_in_dict(MyDetectedEntityInfo info_entity)
        {
            Target target = null;
            // 在字典中查找目标信息
            if (dict__targets_of_raycast.TryGetValue(info_entity.EntityId, out target))
                // 已经存在则更新数据
                target.set(timestamp, info_entity, position);
            else
                // 不存在新建对象
                dict__targets_of_raycast[info_entity.EntityId] = new Target(timestamp, info_entity, position);
        }

        // 锁定多个目标
        void lock_targets()
        {
            // 本轮刷新中扫描到的实体信息集合
            HashSet<MyDetectedEntityInfo> set = new HashSet<MyDetectedEntityInfo>();
            // ID 集合
            HashSet<long> set__id = new HashSet<long>(dict__targets_of_raycast.Keys);
            // 遍历字典中的所有目标对象, 逐个进行锁定
            foreach (var id in set__id)
            {
                Target target = dict__targets_of_raycast[id];

                MyDetectedEntityInfo? entity_info = lock_target(target);
                if (entity_info.HasValue && (flag__register_extra_targets_when_locking || entity_info.Value.EntityId == target.id))
                    // 注册实体信息
                    register_target_in_dict(entity_info.Value);
                if (!entity_info.HasValue || entity_info.Value.EntityId != target.id)
                {
                    if (target.time__until_last_update > delay__after_missing)
                    {
                        // 超过最大等待时间, 视作丢失
                        dict__targets_of_raycast.Remove(id);
                        continue;
                    }
                    // 下一刻
                    target.next_tick(time_span);
                }
            }
        }

        // 尝试锁定目标 (返回实体数据, 可为空)
        MyDetectedEntityInfo? lock_target(Target _target)
        {
            Vector3D p__v, p__a, p__a_avl, p__m, p__m2, position = _target.position;
            //时间
            var time = (timestamp - _target.timestamp) / 1000f;
            p__a_avl = p__a = p__v = position + _target.velocity * time;
            if (!_target.acc.IsZero())
                p__a += 0.5 * _target.acc * time * time;
            if (!_target.acc__average.IsZero())
                p__a_avl += 0.5 * _target.acc__average * time * time;
            // 中点
            p__m = (p__a + p__v) / 2; p__m2 = (position + p__m) / 2;
            RaycastInfo info;
            // 五点探测锁定
            bool flag = cast_at_coordinate(out info, p__a, dict__camera_effectiveness_at_the_time__locking)
                || cast_at_coordinate(out info, p__a_avl, dict__camera_effectiveness_at_the_time__locking)
                || cast_at_coordinate(out info, p__v, dict__camera_effectiveness_at_the_time__locking)
                || cast_at_coordinate(out info, p__m, dict__camera_effectiveness_at_the_time__locking)
                || cast_at_coordinate(out info, p__m2, dict__camera_effectiveness_at_the_time__locking);
            return info.entity_info;
        }

        //输出信息 输出信息到编程块终端和LCD
        void display_info()
        {
            if (!TK.check_time(ref time__before_next_output_update, period__update_output))
                return;
            //清空
            string_builder__title.Clear();
            string_builder__script_info.Clear();
            string_builder__targets.Clear();

            string_builder__title.Append($"<script> ANO-R V0.0.0 {string_array__runtime_patterns[index__crt_char_pattern]}\n");
            string_builder__script_info.Append
            (
                $"\n<count_update> {count__update}"
                + $"\n<count__message_received> {count__message_received}"
                + $"\n<timestamp timespan>  {timestamp} {time_span}"
                + $"\n<flag__under_control flag_scanning> {flag__under_control} {flag__scanning}"
                + $"\n<total_range__effective_for_scanning count__effective_camera_for_scanning> \n{total_range__effective_for_scanning.ToString("0.0")} {count__effective_camera_for_scanning.ToString("0.0")}"
                + $"\n<total_range__effective_for_locking> \n{total_range__effective_for_locking.ToString("0.00")}"
                + $"\n<period__scanning> {period__scanning.ToString("0.00")}"
                + $"\n<period__locking> {period__locking.ToString("0.00")}"
                + $"\n<pid.integral .output__last .error__last> \n {pid_core__scanning.integral.ToString("0.00")} {pid_core__scanning.output__last.ToString("0.00")} {pid_core__scanning.error__last.ToString("0.00")}"
                + $"\n<count__targets_of_auto_turrets> {dict__targets_of_auto_turrets.Keys.Count}"
                + $"\n<count__targets_of_raycast>  {dict__targets_of_raycast.Count}"
                + $"\n<vector__scanning_coordinate>\n    {vector__scanning_coordinate.ToString("0.00")}"
                + $"\n<count_all_blocks> {object_manager__script.list_block.Count}"
                + $"\n<count_controllers count_cameras> {object_manager__script.list_controller.Count} {object_manager__script.list_camera.Count}"
                + $"\n<count_auto_turrets> {object_manager__script.list__auto_turret.Count}\n"
            );

            string_builder__targets.Append($"\n<targets_raycast> ------------------------------ \n\n");
            foreach (var target in dict__targets_of_raycast.Values)
                string_builder__targets.Append(target.get_info());
            string_builder__targets.Append($"\n<targets_turrets> ------------------------------ \n\n");
            foreach (var target in dict__targets_of_auto_turrets.Values)
                string_builder__targets.Append(target.get_info());

            // 显示内容
            Echo(string_builder__title.ToString());
            Echo(string_builder__script_info.ToString());
            Echo(string_builder__targets.ToString());

            //遍历显示单元
            foreach (var item in list__display_units)
                render_displayer(item);

            //显示测试信息
            Echo("\n<debug>\n" + string_builder__test_info.ToString());

            if (string_builder__test_info.Length > 5000)
                string_builder__test_info.Clear();

            if (TK.check_time(ref time__before_next_char_pattern_update, 1))
                if ((++index__crt_char_pattern) >= string_array__runtime_patterns.Length)
                    index__crt_char_pattern = 0;
        }

        // 渲染显示器内容
        void render_displayer(DisplayUnit unit_display)
        {
            if (unit_display.flag_graphic)
            {
                //图形化显示
                if (unit_display.displayer.ContentType != ContentType.SCRIPT)
                    return;
            }
            else
            {
                //非图形化显示
                if (unit_display.displayer.ContentType != ContentType.TEXT_AND_IMAGE)
                    return;
                switch (unit_display.mode_display)
                {
                    case DisplayUnit.DisplayMode.Script:
                    {
                        unit_display.displayer.WriteText(string_builder__title);
                        unit_display.displayer.WriteText(string_builder__script_info, true);
                        break;
                    }
                    case DisplayUnit.DisplayMode.RaycastSingleTarget:
                    {
                        unit_display.displayer.WriteText
                            ($"<numerical_order> No.{unit_display.index_begin} raycast\n" + (list__targets_of_raycast.Count > unit_display.index_begin ?
                            list__targets_of_raycast[unit_display.index_begin].get_info() : string__no_target_info));
                        break;
                    }
                    case DisplayUnit.DisplayMode.TurretSingleTarget:
                    {
                        unit_display.displayer.WriteText
                            ($"<numerical_order> No.{unit_display.index_begin} auto turret\n" + (list__targets_of_auto_turret.Count > unit_display.index_begin ?
                            list__targets_of_auto_turret[unit_display.index_begin].get_info() : string__no_target_info));
                        break;
                    }
                    case DisplayUnit.DisplayMode.RaycastMultipleTargets:
                    {
                        StringBuilder string_builder__temp = new StringBuilder();
                        string_builder__temp.Append($"<numerical_order> No.[{unit_display.index_begin}, {unit_display.index_end}] raycast\n");
                        string_builder__temp.Append(Target.title__simplified_info);
                        for (int i = unit_display.index_begin; i <= unit_display.index_end && i < list__targets_of_raycast.Count; ++i)
                            string_builder__temp.Append(list__targets_of_raycast[i].get_simplified_info());
                        unit_display.displayer.WriteText(string_builder__temp);
                        break;
                    }
                    case DisplayUnit.DisplayMode.TurretMultipleTargets:
                    {
                        StringBuilder string_builder__temp = new StringBuilder();
                        string_builder__temp.Append($"<numerical_order> No.[{unit_display.index_begin}, {unit_display.index_end}] auto turret\n");
                        string_builder__temp.Append(Target.title__simplified_info);
                        for (int i = unit_display.index_begin; i <= unit_display.index_end && i < list__targets_of_auto_turret.Count; ++i)
                            string_builder__temp.Append(list__targets_of_auto_turret[i].get_simplified_info());
                        unit_display.displayer.WriteText(string_builder__temp);
                        break;
                    }
                    case DisplayUnit.DisplayMode.None:
                    {
                        unit_display.displayer.WriteText("<warning> illegal custom data in this LCD\n<by> script ANO-R");
                        break;
                    }
                }
            }
        }

        // 初始化脚本
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
            object_manager__script = new ObjectManager(this);
            // 输出初始化过程中的信息
            Echo(object_manager__script.string_builder__init_info.ToString());

            // 初始化脚本显示单元
            init_script_display_units();

            // 初始化通信
            init_communication();

            // 初始化摄像头评估信息
            init_cameras_evaluation();

            // 检查是否出现错误
            if (str_error == null && !data_config__script.flag__config_error)
                // 设置执行频率
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        // 初始化射线投射单元
        // 将 CD 中第一行字符串相同的摄像头视为相同的组
        // CD 第一行为空的摄像头也会被视为一组的 (或 CD 为空)
        // 若用户拒绝提供摄像头分组信息, 则进行自动识别
        void init_raycast_units()
        {
            if (flag__group_cameras_automatically)
            {
                // 自动对摄像头进行分组
                throw new Exception("You've changed the code! you son of a bitch!");
                // 未实现 unimplemented
            }
            else
            {
                // 
            }
        }

        // 初始化通信
        void init_communication()
        {
            // 注册广播监听器
            listener__scanning_coordinate = IGC.RegisterBroadcastListener
                (string__scanning_coordinate_channel);
            listener__inter_instance_communication = IGC.RegisterBroadcastListener
                (string__internal_communication_channel);
            // 注: 回调是回调自己
            //listener__inter_instance_communication.SetMessageCallback("terminate");
        }

        // 初始化摄像头评估信息
        void init_cameras_evaluation()
        {
            foreach (var camera in object_manager__script.list_camera)
                dict__camera_effectiveness_at_the_time__scanning[camera] = new CameraEvaluationInformation(camera);
        }

        // 初始化脚本显示单元
        void init_script_display_units()
        {
            Dictionary<IMyTextSurface, List<string>> dict = new Dictionary<IMyTextSurface, List<string>>();

            foreach (var item in object_manager__script.list_displayer)
                dict[item as IMyTextSurface] = new List<string>(TK.split_string(item.CustomData));

            foreach (var item in object_manager__script.list__displayer_provider)
            {
                // 以换行拆分
                var list_lines = new List<string>(TK.split_string_2((item as IMyTerminalBlock).CustomData));

                foreach (var line in list_lines)
                {
                    // 拆分行
                    var array_str = new List<string>(TK.split_string(line));
                    int index = 0;

                    if ((!int.TryParse(array_str[0], out index)) || index >= item.SurfaceCount || index < 0)
                        continue; // 解析失败或者索引越界, 跳过
                    array_str.RemoveAt(0);
                    dict[item.GetSurface(index)] = array_str;
                }
            }

            // 遍历字典
            foreach (var kvp in dict)
            {
                //拆分显示器的用户数据
                List<string> array_str = kvp.Value;
                bool flag_illegal = false;
                int offset = 0;

                DisplayUnit unit = new DisplayUnit(kvp.Key);

                if (array_str.Count == 0)
                    unit.mode_display = DisplayUnit.DisplayMode.Script;//用户数据为空
                else
                {
                    //以 "graphic" 结尾
                    if (array_str[array_str.Count - 1].Equals("graphic"))
                    {
                        offset = 1;
                        unit.flag_graphic = true;
                    }

                    //用户数据不为空
                    switch (array_str[0])
                    {
                        case "graphic":
                        case "script":
                            unit.mode_display = DisplayUnit.DisplayMode.Script;
                            break;
                        case "raycast_target":
                        {
                            if (array_str.Count == 2 + offset)//单个
                            {
                                int index = 0;
                                if (!int.TryParse(array_str[1], out index))
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                //边界检查
                                if (index < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                unit.index_begin = index;
                                unit.mode_display = DisplayUnit.DisplayMode.RaycastSingleTarget;
                            }
                            else if (array_str.Count == 3 + offset)//多个
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
                                if (index_begin < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                if (index_end < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                if (index_begin > index_end)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                unit.index_begin = index_begin;
                                unit.index_end = index_end;
                                unit.mode_display = DisplayUnit.DisplayMode.RaycastMultipleTargets;
                            }
                            else
                                flag_illegal = true;
                            break;
                        }
                        case "turret_target":
                        {
                            if (array_str.Count == 2 + offset)//单个
                            {
                                int index = 0;
                                if (!int.TryParse(array_str[1], out index))
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                //边界检查
                                if (index < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                unit.index_begin = index;
                                unit.mode_display = DisplayUnit.DisplayMode.TurretSingleTarget;
                            }
                            else if (array_str.Count == 3 + offset)//多个
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
                                if (index_begin < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                if (index_end < 0)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                if (index_begin > index_end)
                                {
                                    flag_illegal = true;
                                    break;
                                }
                                unit.index_begin = index_begin;
                                unit.index_end = index_end;
                                unit.mode_display = DisplayUnit.DisplayMode.TurretMultipleTargets;
                            }
                            else
                                flag_illegal = true;
                            break;
                        }
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
                    kvp.Key.ContentType = ContentType.SCRIPT;//设置为 脚本模式
                    kvp.Key.Script = "";//脚本设为 None
                    kvp.Key.ScriptBackgroundColor = Color.Black;//黑色背景色
                }
                else
                    kvp.Key.ContentType = ContentType.TEXT_AND_IMAGE;//设置为 文字与图片模式

                //添加到列表
                list__display_units.Add(unit);
            }
        }

        //初始化脚本配置
        void init_script_config()
        {
            //脚本配置实例
            data_config__script = new DataConfig("ANO-L");
            data_config__script.set_string(() => Me.CustomData, (x) => Me.CustomData = x);

            data_config_set__script = new DataConfigSet("AN0-R CONFIGURATION");

            //添加配置项

            data_config_set__script.add_annotation("the core group name of script to be identified");
            data_config_set__script.add_item((new Variant(nameof(name__script_core_group), Variant.VType.String, () => name__script_core_group, x => { name__script_core_group = (string)x; })));

            data_config_set__script.add_annotation("channels for inter-script communication");
            data_config_set__script.add_item((new Variant(nameof(string__internal_communication_channel), Variant.VType.String, () => string__internal_communication_channel, x => { string__internal_communication_channel = (string)x; })));
            data_config_set__script.add_item((new Variant(nameof(string__internal_communication_channel), Variant.VType.String, () => string__internal_communication_channel, x => { string__internal_communication_channel = (string)x; })));
            data_config_set__script.add_item((new Variant(nameof(string__scanning_coordinate_channel), Variant.VType.String, () => string__scanning_coordinate_channel, x => { string__scanning_coordinate_channel = (string)x; })));

            data_config_set__script.add_annotation("function switches of the script (`True` or `False`)");
            data_config_set__script.add_item((new Variant(nameof(flag__obtain_auto_turret_targets), Variant.VType.Bool, () => flag__obtain_auto_turret_targets, x => { flag__obtain_auto_turret_targets = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__enable_targets_localcasting_while_offline), Variant.VType.Bool, () => flag__enable_targets_localcasting_while_offline, x => { flag__enable_targets_localcasting_while_offline = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__enable_unlimited_raycast), Variant.VType.Bool, () => flag__enable_unlimited_raycast, x => { flag__enable_unlimited_raycast = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__enable_inter_instance_communication), Variant.VType.Bool, () => flag__enable_inter_instance_communication, x => { flag__enable_inter_instance_communication = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__receiving_scanning_coordinate), Variant.VType.Bool, () => flag__receiving_scanning_coordinate, x => { flag__receiving_scanning_coordinate = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__register_extra_targets_when_locking), Variant.VType.Bool, () => flag__register_extra_targets_when_locking, x => { flag__register_extra_targets_when_locking = (bool)x; })));

            data_config_set__script.add_annotation("filters used to filter the type of targets scanned with raycast (`True` or `False`)");
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_players), Variant.VType.Bool, () => flag__ignore_players, x => { flag__ignore_players = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_rockets), Variant.VType.Bool, () => flag__ignore_rockets, x => { flag__ignore_rockets = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_meteors), Variant.VType.Bool, () => flag__ignore_meteors, x => { flag__ignore_meteors = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_small_grids), Variant.VType.Bool, () => flag__ignore_small_grids, x => { flag__ignore_small_grids = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_large_grids), Variant.VType.Bool, () => flag__ignore_large_grids, x => { flag__ignore_large_grids = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_the_friendly), Variant.VType.Bool, () => flag__ignore_the_friendly, x => { flag__ignore_the_friendly = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_the_neutral), Variant.VType.Bool, () => flag__ignore_the_neutral, x => { flag__ignore_the_neutral = (bool)x; })));
            data_config_set__script.add_item((new Variant(nameof(flag__ignore_the_enemy), Variant.VType.Bool, () => flag__ignore_the_enemy, x => { flag__ignore_the_enemy = (bool)x; })));

            data_config_set__script.add_annotation("the period during which certain actions are performed (in frames)");
            data_config_set__script.add_item(new Variant(nameof(period__auto_check), Variant.VType.Int, () => period__auto_check, x => { period__auto_check = (long)x; }));
            data_config_set__script.add_item(new Variant(nameof(period__update_output), Variant.VType.Int, () => period__update_output, x => { period__update_output = (long)x; }));
            data_config_set__script.add_item(new Variant(nameof(period__update_targets_of_auto_turrets), Variant.VType.Int, () => period__update_targets_of_auto_turrets, x => { period__update_targets_of_auto_turrets = (long)x; }));

            data_config_set__script.add_annotation("the maximum time to wait before the missing target is deleted (in frames)");
            data_config_set__script.add_item(new Variant(nameof(delay__after_missing), Variant.VType.Int, () => delay__after_missing, x => { delay__after_missing = (long)x; }));

            data_config_set__script.add_annotation("distance parameters associated with scanning (in meters)");
            data_config_set__script.add_item(new Variant(nameof(distance__min), Variant.VType.Float, () => distance__min, x => { distance__min = (double)x; }));
            data_config_set__script.add_item(new Variant(nameof(distance__max), Variant.VType.Float, () => distance__max, x => { distance__max = (double)x; }));
            data_config_set__script.add_item(new Variant(nameof(distance__scan), Variant.VType.Float, () => distance__scan, x => { distance__scan = (double)x; }));

            data_config_set__script.add_item(new Variant(nameof(frequency__min), Variant.VType.Float, () => frequency__min, x => { frequency__min = (double)x; }));
            data_config_set__script.add_item(new Variant(nameof(base__log), Variant.VType.Float, () => base__log, x => { base__log = (double)x; }));

            // 添加配置集合
            data_config__script.add_config_set(data_config_set__script);

            //初始化配置
            data_config__script.init_config();
            // 输出配置初始化过程中的信息
            Echo(data_config__script.string_builder__error_info.ToString());

        }

        //检查脚本配置(配置合法返回null, 否则返回错误消息)
        string check_config()
        {
            if (name__script_core_group.Length == 0)
                return "<error_config> key tag of group name is empty";
            if (!TK.check_value(period__auto_check) || !TK.check_value(period__update_output))
                return "<error_config> period cannot be less than 1 or more than 1000";
            return null;
        }

        #endregion

        #region 类型定义

        // 结构体 交换信息
        // 交换信息封装了用于交换的信息, 内部只有一个经过嵌套的元组对象
        struct ExchangeInfo
        {
            // 信息 <实体信息, 第一次投射碰撞点, 预测位置, 加速度, 平均加速度, <距上次更新已过刻数, 机动性, 距离, 最大加速度>>
            public MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>> data;

            // 隐式转换
            public static implicit operator ExchangeInfo
                (MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>> tuple)
            => new ExchangeInfo { data = tuple };

            // 隐式转换
            public static implicit operator
                MyTuple<MyDetectedEntityInfo, Vector3D?, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>>(ExchangeInfo info)
            => info.data;
        }

        //类 显示单元
        class DisplayUnit
        {
            //枚举 显示模式
            public enum DisplayMode
            {
                Script,// 脚本主要信息
                TurretSingleTarget,// 炮塔目标
                TurretMultipleTargets,// 炮塔多个目标
                RaycastSingleTarget,// 射线投射单个目标
                RaycastMultipleTargets,// 炮塔多个目标
                RadarMap,// 雷达图
                None,// 不显示内容
            }

            //显示器
            public IMyTextSurface displayer;

            public DisplayMode mode_display = DisplayMode.Script;

            //索引 开始 (列表容器中的索引)
            public int index_begin = -1;
            //索引 末尾 (列表容器中的索引)
            public int index_end = -1;
            //标记 是否图形化显示
            public bool flag_graphic = false;

            //构造函数
            public DisplayUnit(IMyTextSurface _display, int _index_begin = -1, int _index_end = -1, bool _flag_graphic = false)
            {
                displayer = _display;
                flag_graphic = _flag_graphic;
                index_begin = _index_begin;
                index_end = _index_end;
            }
        }

        //类 目标
        class Target
        {
            public MyDetectedEntityInfo info__entity;// 实体信息
            public long timestamp { get; private set; }// 时间戳 (上一次更新时的时间戳, 当前脚本的时间戳) (-1 表示对象不可用, -2 表示该目标经由脚本间通信获取)
            public long time__until_last_update { get; private set; }
            public double level__maneuverability { get; private set; }// 机动性
            public Vector3D position { get; private set; }// 预测位置 (使用 next_tick() 函数将会更新这个值)
            public Vector3D? position__first_hit { get; }// 第一次的投射命中点, 若并非由投射获取则此值为 null
            public Vector3D acc { get; private set; }// 瞬时加速度
            public Vector3D acc__average { get; private set; }// 平均加速度 (短期内平均值)
            public double acc__maximum { get; private set; } = 0.0;// 最大加速度值
            public double distance { get; private set; }// 当前与本机的距离 (扫描后才更新)
            public double distance__at_dicovered { get; private set; }// 发现目标时与本机的距离
            public double distance__average { get; private set; }// 与本机的平均距离


            // 一些快捷属性
            public double speed => info__entity.Velocity.Length(); // 速率
            public long id => info__entity.EntityId;// ID
            public Vector3D velocity => info__entity.Velocity;// 速度
            public string name => info__entity.Name;// 名称

            // 构造函数 (时间戳, 实体信息, 本机位置)
            public Target(long _timestamp, MyDetectedEntityInfo _i, Vector3D _p)
            {
                // 若存在值
                if (_i.HitPosition.HasValue)
                    position__first_hit = _i.HitPosition.Value - _i.Position;
                set(_timestamp, _i, _p);
            }

            // 默认构造函数 (构造的对象为空)
            public Target() { timestamp = -1; }

            public Target(MyTuple<MyDetectedEntityInfo, Vector3D, Vector3D, Vector3D, MyTuple<long, double, double, double>> _data)
            {
                this.info__entity = _data.Item1;
                this.timestamp = timestamp;
                this.time__until_last_update = time__until_last_update;
                this.level__maneuverability = level__maneuverability;
                this.position = position;
                this.acc = acc;
                this.acc__average = acc__average;
                this.acc__maximum = acc__maximum;
                this.distance__at_dicovered = this.distance__average = distance__at_dicovered;
            }

            // 转为交换信息结构体实例
            public ExchangeInfo to_exchange_info()
                => new ExchangeInfo
                {
                    data = MyTuple.Create(info__entity, position__first_hit, position, acc, acc__average,
                    MyTuple.Create(time__until_last_update, level__maneuverability, distance__at_dicovered, acc__maximum))
                };
            public Vector3D next_tick(long ts)
            {
                ++time__until_last_update;
                // 不考虑目标的加速度
                return position = position + info__entity.Velocity * (ts / 1000f);
            }

            // 设置对象 (当前时间戳, 目标信息, 当前脚本所在位置)
            public void set(long _timestamp__crt, MyDetectedEntityInfo _info, Vector3D _position)
            {
                // 检查是否是新的对象
                if (info__entity.EntityId == _info.EntityId)
                { // 旧对象
                    acc = (_timestamp__crt == timestamp) ? Vector3D.Zero : new Vector3D((_info.Velocity - info__entity.Velocity) * 1000f / (_timestamp__crt - timestamp));
                    var t = TK.cal_smooth_avg(acc__average.Length(), acc.Length());
                    acc__average = acc__average + t * (acc - acc__average);
                    acc__maximum = Math.Max(acc.Length(), acc__maximum);
                }
                else // 新对象
                    acc = acc__average = Vector3D.Zero;
                timestamp = _timestamp__crt;
                time__until_last_update = 0;
                info__entity = _info;
                position = info__entity.Position;
                distance__at_dicovered = distance__average = (_position - position).Length();
                //speed = info_entity.Velocity.Length();
            }
            public string get_info() => timestamp == -1 ? "<target> invalid\n\n" :
                $"<target> --------------------"
                + $"\n<name type> {name} {info__entity.Type}"
                + $"\n<id> {id}"
                + $"\n<source> {(timestamp == -2 ? "communication" : "localhost")}"
                + $"\n<timestamp last_update> {(timestamp == -2 ? "None" : timestamp.ToString())} {(time__until_last_update / 60.0f).ToString("0.00")}"
                + $"\n<distance> {TK.nf(distance__at_dicovered)} m"
                + $"\n<position>\n{position.ToString("0.00E0")}"
                + $"\n<speed> {TK.nf(speed)} m/s"
                + $"\n<velocity> {info__entity.Velocity.ToString("0.00")}"
                + $"\n<acc acc_avg> {TK.nf(acc.Length())} {TK.nf(acc__average.Length())}\n\n";
            public string get_simplified_info() => timestamp == -1 ? "<target> invalid\n" :
                $"<target> {name} {TK.nf(distance__at_dicovered)}m {TK.nf(speed)} m/s\n";
            public static string title__simplified_info => "<title> name distance speed ----------\n\n";
        }

        // 投射信息, 不要用结构体, 会变得不幸
        class RaycastInfo
        {
            // 摄像头 施放投射的摄像头
            public IMyCameraBlock camera { get; private set; }
            // 投射距离
            public double distance { get; private set; }
            // 扫描到的对象信息
            public MyDetectedEntityInfo? entity_info { get; private set; }
            // 投射角度差
            public double angle { get; private set; }
            // 标记 是否成功投射
            public bool flag__did_raycasted { get; private set; }

            // 默认构造函数, 将构造一个不具有有效性的实例
            public RaycastInfo()
            {
                camera = null;
                distance = angle = -1;
                entity_info = null;
                flag__did_raycasted = false;
            }

            // 标准构造函数
            public RaycastInfo(IMyCameraBlock _camera, double _distance, MyDetectedEntityInfo? _entity_info, double _angle, bool _flag__did_raycasted)
            {
                this.camera = _camera;
                this.distance = _distance;
                this.entity_info = _entity_info;
                this.angle = _angle;
                this.flag__did_raycasted = _flag__did_raycasted;
            }
        }

        // 摄像头评估信息
        class CameraEvaluationInformation
        {

            /**
			 * 分配策略 (时间复杂度最高不可超过 O(n))
			 * 首先, 不可能每一帧都对摄像头进行排序, 当摄像头数量过多时开销过大
			 * 采样折中方案, 当一个摄像头投射之后, 记录其优先级, 并将优先级重置为 0
			 * 下一次选择优先级大于上一次选择的摄像头的优先级 60% 的摄像头
			 * (设置为 60% 或者更低的值是为了放宽限制, 尽量避免避免所有摄像头都无法满足需求, 
			 * 同时因为投射过的摄像头优先级被置为0, 因此不太可能仍然选择上一次的摄像头)
			 * 若找不到则选择一个能投射的摄像头, 顺序循环时发现可以投射的摄像头时先不进行投射
			 * 判断其优先级是否足够, 若足够则直接进行投射, 否则继续查找下一个
			 * 若找完一圈都没有发现优先级足够高的摄像头则选择之前记录的摄像头进行投射
			 * 
			 */
            // 摄像头
            public IMyCameraBlock camera { get; private set; }
            // 有效性, 有效性是指对于投射目标而言, 摄像头可能处于射界内的可能性的评估
            public double effectiveness { get; private set; }
            // 优先级, 当一个摄像头被使用之后其优先级会降低
            public double priority { get; private set; }
            // 当前射程
            public double range => camera.AvailableScanRange;

            public CameraEvaluationInformation(IMyCameraBlock _camera)
            {
                this.camera = _camera;
                // 初始时将摄像头的有效性视为 0.5
                effectiveness = 0.5;
                // 优先级初始均为 1
                priority = 1;
            }

            // 更新, 返回当前摄像头的有效性射程评估值 (有效性 * 总射程)
            // [out] _effectiveness 返回有效性
            public double update(out double _effectiveness)
            {
                _effectiveness = effectiveness;
                double result = effectiveness * camera.AvailableScanRange;
                // 有效性将线性减少
                effectiveness -= 0.001;
                if (effectiveness < 0)
                    effectiveness = 0;
                // 即使当有效性为0时优先级仍然会缓慢增长, 若有效性较高, 此时能量也会加速优先级增长
                priority += 0.001 + effectiveness * (0.002 + camera.AvailableScanRange / 100000);
                return result;
            }

            // 投射后更新, 传入投射夹角
            // [in] _angle 当前与目标的角度
            // [in] _flag__reset_priority 是否重置优先级 (投射是否真正发生)
            public void update_after_cast(double _angle, bool _flag__reset_priority)
            {
                // 进行一次投射之后优先级会降低, 以尽可能降低其被调用的概率
                if (_flag__reset_priority)
                    priority = 0;
                effectiveness = Math.Min(1, effectiveness + Math.Max(0, 1 - (_angle / camera.RaycastConeLimit)));
            }

        }

        /**************************************************************************
		* 类 ObjectsManager
		* 对象管理器
		* 实例中包含本脚本所需的全部对象列表
		**************************************************************************/
        class ObjectManager
        {
            // 编组 脚本核心方块
            private IMyBlockGroup group__script_core = null;

            // 列表 所有方块
            public List<IMyTerminalBlock> list_block { get; private set; } = new List<IMyTerminalBlock>();
            // 列表 全部机械连接方块
            public List<IMyMechanicalConnectionBlock> list_mcb { get; private set; } = new List<IMyMechanicalConnectionBlock>();
            // 列表 全部控制器
            public List<IMyShipController> list_controller { get; private set; } = new List<IMyShipController>();
            // 列表 全部摄像头
            public List<IMyCameraBlock> list_camera { get; private set; } = new List<IMyCameraBlock>();
            // 列表 全部自动炮塔
            public List<IMyLargeTurretBase> list__auto_turret { get; private set; } = new List<IMyLargeTurretBase>();
            // 列表 显示器
            public List<IMyTextPanel> list_displayer { get; private set; } = new List<IMyTextPanel>();
            // 列表 显示器提供者
            public List<IMyTextSurfaceProvider> list__displayer_provider { get; private set; } = new List<IMyTextSurfaceProvider>();

            // 哈希表 自身网格的ID
            public HashSet<long> set__self_id { get; private set; } = new HashSet<long>();

            //字典 根据网格快速检索节点索引
            private Dictionary<IMyCubeGrid, long> dict__grid_index = new Dictionary<IMyCubeGrid, long>();

            // 脚本实例
            private readonly Program p = null;

            //字符串构建器 初始化信息
            public StringBuilder string_builder__init_info { get; private set; } = new StringBuilder();

            //构造函数
            public ObjectManager(Program _p)
            {
                //成员赋值
                p = _p;
                //获取脚本核心编组
                group__script_core = p.GridTerminalSystem.GetBlockGroupWithName(p.name__script_core_group);
                if (group__script_core == null)
                    return;
                group__script_core.GetBlocks(list_block);
                group__script_core.GetBlocksOfType(list_controller);
                group__script_core.GetBlocksOfType(list_camera);
                group__script_core.GetBlocksOfType(list__auto_turret);
                group__script_core.GetBlocksOfType(list_displayer);
                group__script_core.GetBlocksOfType(list__displayer_provider);

                foreach (var i in list_camera)
                    i.EnableRaycast = true;//启用投射
                if (p.flag__enable_unlimited_raycast)
                    foreach (var i in list_camera)
                        i.Raycast(0d / 0d);//进行一次测试性投射

                //获取全部机械连接方块(电机, 铰链, 活塞, 悬架)
                p.GridTerminalSystem.GetBlocksOfType(list_mcb);

                //long index = 0;
                //获取全部网格 构建节点列表和节点索引
                foreach (var item in list_mcb)
                {
                    set__self_id.Add(item.CubeGrid.EntityId);
                    if (item.TopGrid != null)
                        set__self_id.Add(item.TopGrid.EntityId);
                }

                generate_init_info();
            }

            //生成初始化信息
            private void generate_init_info()
            {
                string_builder__init_info.Append($"<info> list_blocks.Count = {list_block.Count}\n");
                string_builder__init_info.Append($"<info> list_cameras.Count = {list_camera.Count}\n");
                string_builder__init_info.Append($"<info> list_controllers.Count = {list_controller.Count}\n");
                string_builder__init_info.Append($"<info> list__auto_turrets.Count = {list__auto_turret.Count}\n");
                string_builder__init_info.Append($"<info> list_displayers.Count = {list_displayer.Count}\n");

                //buinder_str__init_info.Append($"<info> list__other_blocks.Count = {list__other_blocks.Count}\n");
            }
        }

        /**************************************************************************
		* 类 RaycastUnit
		* 射线投射单元
		* 同一个摄像头编组内的摄像头被视为在空间位置上非常相似
		* (相近的朝向, 同时物理位置距离不远, 因此依赖用户提供的信息)
		* 本脚本进行摄像头调度的最小单元, 其接口应该使其表现得像一个摄像头
		* 摄像头单元的射界是以其内的摄像头的射界为基础的
		* 通常我们认为当一个目标在其内一定数量个摄像头的射界之外后便不考虑这个单元
		* 比如说两个, 这样我们用少数次的射界检查便可以避免检查所有的摄像头
		* 在实现上, 可以考虑随机抽取一定数量的摄像头作为射界检查
		* 粗略射界检查: 
		* 单元内的所有摄像头的平均朝向为基准向量, 平均位置为原点, 判断 [目标-原点] 与 [平均朝向] 的夹角
		* 夹角超过一定程度则视为不通过检查
		**************************************************************************/
        class RaycastUnit
        {
            public List<IMyCameraBlock> list__camera_in_group { get; private set; } // 摄像头单元所管理的摄像头


            // 构造函数
            public RaycastUnit(List<IMyCameraBlock> _list__camera_in_group)
            {
                list__camera_in_group = _list__camera_in_group;
            }




        }

        static class TK
        {
            //计算平滑均值 (上个平滑值, 当前值, 增量(增量为null使用默认的减法计算))
            //逼近新的值, 变化越大惩罚越大, inc为倍数增量的比率0.5*[0,1]
            public static double cal_smooth_avg(double l, double c, double? i = null)
            {
                double inc = 0.5 * ((inc = Math.Abs((i == null ? c - l : i.Value) / l)) > 1 ? 1 : inc);
                return double.IsNaN(inc) ? l : (inc * l + (1 - inc) * c);
            }

            // 全局 (朝向) 转 yaw pitch
            // 注: 摄像头欧拉角顺序: Y -> P
            public static void global_to_yaw_pitch(out float yaw, out float pitch, MatrixD matrix, Vector3D tgt)
            {
                // 获取三个方向向量
                Vector3D v_l = matrix.Left, v_u = matrix.Up, v_f = matrix.Forward;

                // 计算目标向量在水平面 (法线为 U 向量) 的投影
                var p2u = Vector3D.ProjectOnPlane(ref tgt, ref v_u);

                // 求在水平面的投影与前向夹角 (conθ = 点积 / 模长积)
                double num = Vector3D.Dot(p2u, v_f) / (p2u.Length() * v_f.Length());
                // 去除精度问题
                if (num > 1.0)
                    num = 1.0;
                if (num < -1.0)
                    num = -1.0;
                // 计算弧度, 之后转角度
                yaw = (float)(Math.Acos(num) * 180 / Math.PI);
                if (Vector3D.Dot(v_l, p2u) > 0)
                    yaw = -yaw;

                // 求在水平面的投影与目标向量的夹角
                num = Vector3D.Dot(p2u, tgt) / (p2u.Length() * tgt.Length());
                if (num > 1.0)
                    num = 1.0;
                if (num < -1.0)
                    num = -1.0;
                // 计算弧度, 之后转角度
                pitch = (float)(Math.Acos(num) * 180 / Math.PI);
                if (Vector3D.Dot(v_u, tgt) < 0)
                    pitch = -pitch;
            }

            // 检查是否到时间执行某步骤
            public static bool check_time(ref long time, long period)
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

            // 转字符串(保留2位小数)
            public static string nf(double d) => d.ToString("0.00");

            // 计算周期 (周期与能量的对数呈负相关)
            // 内部使用 PID, 尽可能将能量维持在一个稳定的水平
            // 基准周期 = 平均长度 / 平均增量
            // 当前误差 = 当前能量 - 预期能量 (误差用于输入 PID)
            // [in] 当前能量, 预期能量, 当前增量, 最小频率, 平均投射距离, PID 核心对象
            // [return] 返回计算得出的周期值, 最小不会低于1, 最大不会超过 _frequency__min 对应的周期
            public static double calculate_period
                (double _energe__current, double _energe__expected, double _increment__average, double _frequency__min, double _distance__average, PidCore _pid_core)
            {
                // 注: PID 传入的误差是当前能量与期望能量的差, 再除以平均投射长度, 也就是说这个误差实际上是 "误差次数"
                double result = Math.Max
                    (1.0f, Math.Min((60 / _frequency__min), (_distance__average / _increment__average + (_pid_core.cal_output((_energe__current - _energe__expected) / _distance__average)))));

                if (double.IsNaN(result))
                {
                    throw new Exception
                        (
                        $"\n<info> NaN period" +
                        $"\n<_energe__current> {_energe__current}" +
                        $"\n<_energe__expected> {_energe__expected}" +
                        $"\n<_increment__average> {_increment__average}" +
                        $"\n<_frequency__min> {_frequency__min}" +
                        $"\n<_distance__average> {_distance__average}" +
                        $"\n<_pid_core.error_last> {_pid_core.error__last}" +
                        $"\n<_pid_core.output_last> {_pid_core.output__last}" +
                        $"\n<_pid_core.integral> {_pid_core.integral}" +
                        $"\n<_pid_core.threshold__integral_separation> {_pid_core.threshold__integral_separation}" +
                        $"\n<_pid_core.lower_limit__output> {_pid_core.lower_limit__output}" +
                        $"\n<_pid_core.upper_limit__output> {_pid_core.upper_limit__output}" +
                        $"\n<_pid_core.coefficient__proportion> {_pid_core.coefficient__proportion}" +
                        $"\n<_pid_core.coefficient__integral> {_pid_core.coefficient__integral}" +
                        $"\n<_pid_core.coefficient__differential> {_pid_core.coefficient__differential}" +
                        $"\n<_pid_core.coefficient__dynamic_integral_ratio> {_pid_core.coefficient__dynamic_integral_attenuation_ratio}" +
                        $"\n<_pid_core.coefficient__dynamic_integral_exponent> {_pid_core.coefficient__dynamic_integral_max_ratio}" +
                        ""
                        );
                }

                return result;

            }

            //检查数值
            public static bool check_value(long value) => value > 0 && value < 1001;

            // 计算向量夹角 (弧度制)
            public static double calculate_angle(Vector3D vec__0, Vector3D vec__1)
                => Math.Acos(Vector3D.Dot(vec__0, vec__1) / vec__0.Length() / vec__1.Length());

            //拆分字符串
            public static string[] split_string(string str) => str.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            public static string[] split_string_2(string str) => str.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }



        /**************************************************************************
		* 类 PIDCore
		* PID 核心
		* 
		* 增强
		* 比例系数:
		* 
		* 微分系数:
		* 
		* 积分系数:
		* [0] 积分抗饱和
		*       某一侧输出到达极限时不进行同侧积分
		* [1] 积分分离
		*       误差超过阈值时停止积分
		* [2] 变速积分
		*       误差越大积分作用越小, 否则越大
		* 
		**************************************************************************/
        class PidCore
        {
            // 分别是: 比例系数 (p), 积分系数 (i), 微分系数 (d)
            public double coefficient__proportion { get; set; } = 100;
            public double coefficient__integral { get; set; } = 25;
            public double coefficient__differential { get; set; } = 50;

            // 积分衰减速度
            // 1 表示标准衰减速度, <1 衰减更慢, >1 衰减更快
            public double coefficient__dynamic_integral_attenuation_ratio { get; set; } = 1;
            // 积分最高倍率, 值必须是一个正数, 相当于当误差趋近于 0 时, 增加积分的倍率
            public double coefficient__dynamic_integral_max_ratio { get; set; } = 1;

            // 上限 输出上限
            public double upper_limit__output { get; set; } = 10;
            // 下限, 输出下限
            public double lower_limit__output { get; set; } = -10;
            // 阈值 积分分离阈值 (也就是当误差绝对值大于这个值之后当次不再进行积分操作)
            public double threshold__integral_separation { get; set; } = 5;
            // 积分 最大值, 累计的积分的绝对值不可超过这个数字
            public double limit__integral { get; set; } = 20;

            // 标记 启用微分项
            public bool flag__enable_derivative_term { get; set; } = true;
            // 标记 启用积分项
            public bool flag__enable_integral_term { get; set; } = true;
            // 标记 启用积分抗饱和
            public bool flag__enable_integral_anti_windup { get; set; } = true;
            // 标记 启用积分分离 (当误差特别大的时候, 不进行积分操作, 避免干扰)
            public bool flag__enable_integral_separation { get; set; } = true;
            // 标记 启用动态积分 (变速积分)
            public bool flag__enable_dynamic_integral { get; set; } = true;
            // 标记 启用积分上限 (保证积分的绝对值不超过上限)
            public bool flag___enable_upper_limit_of_integral { get; set; } = true;

            // 调用计数
            public int count_invoke { get; private set; } = 0;

            // 积分值, 积分值相当于对误差的累计计算
            public double integral { get; private set; } = 0.0;
            // 误差值 上一次误差
            public double error__last { get; private set; } = 0.0;
            // 输出值 上一次的输出值
            public double output__last { get; private set; } = 0.0;

            // 构造函数
            public PidCore()
            {

            }

            public PidCore
            (
                double _coefficient__proportion,
                double _coefficient__integral,
                double _coefficient__differential,
                double _coefficient__dynamic_integral_attenuation_ratio,
                double _coefficient__dynamic_integral_max_ratio,
                double _upper_limit__output,
                double _lower_limit__output,
                double _threshold__integral_separation,
                double _limit__integral,
                bool _flag__enable_derivative_term = true,
                bool _flag__enable_integral_term = true,
                bool _flag__enable_integral_anti_windup = true,
                bool _flag__enable_integral_separation = true,
                bool _flag__enable_dynamic_integral = true,
                bool _flag___enable_upper_limit_of_integral = true
            )
            {
                this.coefficient__proportion = _coefficient__proportion;
                this.coefficient__integral = _coefficient__integral;
                this.coefficient__differential = _coefficient__differential;
                this.coefficient__dynamic_integral_attenuation_ratio = _coefficient__dynamic_integral_attenuation_ratio;
                this.coefficient__dynamic_integral_max_ratio = _coefficient__dynamic_integral_max_ratio;
                this.upper_limit__output = _upper_limit__output;
                this.lower_limit__output = _lower_limit__output;
                this.threshold__integral_separation = _threshold__integral_separation;
                this.limit__integral = _limit__integral;
                this.flag__enable_derivative_term = _flag__enable_derivative_term;
                this.flag__enable_integral_term = _flag__enable_integral_term;
                this.flag__enable_integral_anti_windup = _flag__enable_integral_anti_windup;
                this.flag__enable_integral_separation = _flag__enable_integral_separation;
                this.flag__enable_dynamic_integral = _flag__enable_dynamic_integral;
                this.flag___enable_upper_limit_of_integral = _flag___enable_upper_limit_of_integral;
            }

            // 计算输出
            // [in] error 当前误差
            // [return]   当前误差对应的负反馈输出
            public double cal_output(double error)
            {
                double derivative = (this.flag__enable_derivative_term) ? (error - error__last) : 0.0;
                // 开启积分 && 积分分离 (超过阈值不进行积分) && 积分抗饱和 (若输出达到上限, 则误差低于0时不积分; 输出超过上限意味着 error 长期为负数, 因此负 error 不再累加入积分, 防止过饱和)
                if (this.flag__enable_integral_term && (flag__enable_integral_separation ? (Math.Abs(error) < threshold__integral_separation) : true))
                    if (flag__enable_integral_anti_windup ? ((output__last > upper_limit__output) ? (error > 0) : (output__last < lower_limit__output ? (error < 0) : true)) : true)
                        integral += (flag__enable_dynamic_integral ? (error / (coefficient__dynamic_integral_attenuation_ratio * Math.Abs(error) + 1 / coefficient__dynamic_integral_max_ratio)) : error);
                if (flag___enable_upper_limit_of_integral) integral = Math.Max(-limit__integral, Math.Min(limit__integral, integral));
                ++count_invoke; error__last = error; output__last = -(coefficient__proportion * error + coefficient__integral * integral + coefficient__differential * derivative);
                return Math.Max(lower_limit__output, Math.Min(upper_limit__output, output__last));
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
            public static string identifier_annotation_0 = "# ";
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

            //添加注解
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
                    if (pair.Value != null)
                        string_builder__data.Append(pair.Value);
                    else
                    {
                        if (count != 0)
                            string_builder__data.Append("\n");
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