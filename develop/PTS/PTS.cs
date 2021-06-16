/***************************************************************************************************
*
* ### In-Game Script Development Template (脚本英文全称) ###
* ### IGSDT(脚本英文缩写) | 这里填写脚本中文名 --------- ###
* ### Version 0.0.0(脚本版本号) | by XXXXXXX(你的名字) - ###
* ### STPC旗下SCP脚本工作室开发 欢迎加入STPC ----------- ###
* ### STPC主群群号:320461590 我们欢迎新朋友 ------------ ###
* 
* STPC组织仓库网址:
* https://github.com/stpc-org
* (这里可以放脚本的说明书网址)
* 
***************************************************************************************************/

//在游戏内使用脚本时确保下面一行代码被注释
//在开发过程中取消注释来获取完整的代码提示和代码补全
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

//TODO: 在这里设置一个命名空间名称, 名称随意但保证不同脚本的命名空间不同
//我推荐使用全大写的下划线命名方式
namespace PTS
{
	class Program:MyGridProgram
	{
#endif

		#region 脚本字段

		string name_group = "Pistons";

		IMyBlockGroup group;

		List<IMyPistonBase> list_pistons = new List<IMyPistonBase>();


		#endregion

		#region 脚本入口

		/***************************************************************************************************
		* 构造函数 Program()
		***************************************************************************************************/
		public Program()
		{
			Echo("<execute> init");

			group=this.GridTerminalSystem.GetBlockGroupWithName(name_group);

			if(group==null)
				Echo($"<error> no group found with name: {name_group}");
			else
				group.GetBlocksOfType(list_pistons);

			if(list_pistons.Count==0)
				Echo($"<error> no pistons in group");

			ITerminalProperty p;

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
				{

					var cmd = split_string(str_arg);//空格拆分

					if(cmd.Count==0)
						return;
					switch(cmd[0])
					{
						case "attach":
						{
							if(group!=null)
							{
								foreach(var i in list_pistons)
								{
									i.Detach();
									i.Attach();
								}
							}
						}
						break;
						case "detach":
						{
							if(group!=null)
							{
								foreach(var i in list_pistons)
								{
									i.Detach();
								}
							}
						}
						break;

					}
				}
				break;
				case UpdateType.Update1:
				case UpdateType.Update10:
				case UpdateType.Update100:
				{

				}
				break;
			}
		}

		List<string> split_string(string str)
		{
			List<string> list = new List<string>(str.Split(new string[] { " " },StringSplitOptions.RemoveEmptyEntries));
			return list;
		}

		#endregion

#if DEVELOP
	}
}
#endif
