--下面基础组件间的require有依赖顺序相关,闲着没事也别换顺序,要加新的往文件尾加就好
Util = CS.XLuaFramework.Util
AppConfig = CS.XLuaFramework.AppConfig
ResMgr = CS.XLuaFramework.ResourceManager.GetInstance()
NetMgr = CS.XLuaFramework.NetworkManager.GetInstance()
UIHelper = CS.XLuaFramework.UIHelper
cookiesManager = CS.XLuaFramework.CookiesManager.GetInstance()
print('Cat:BaseRequire.lua[8] cookiesManager', cookiesManager)
GameObject = CS.UnityEngine.GameObject
GameConst = CS.UnityMMO.GameConst
GameVariable = CS.UnityMMO.GameVariable
SceneMgr = CS.UnityMMO.SceneMgr

Mathf		= require "Common.UnityEngine.Mathf"
Vector2		= require "Common.UnityEngine.Vector2"
Vector3 	= require "Common.UnityEngine.Vector3"
Vector4		= require "Common.UnityEngine.Vector4"
Quaternion	= require "Common.UnityEngine.Quaternion"
Color		= require "Common.UnityEngine.Color"
Ray			= require "Common.UnityEngine.Ray"
Bounds		= require "Common.UnityEngine.Bounds"
RaycastHit	= require "Common.UnityEngine.RaycastHit"
Touch		= require "Common.UnityEngine.Touch"
LayerMask	= require "Common.UnityEngine.LayerMask"
Plane		= require "Common.UnityEngine.Plane"
Time		= require "Common.UnityEngine.Time"
Object		= require "Common.UnityEngine.Object"

require("Common/BaseClass")
require("Common.Util.util")
require("Common.Util.LuaUtil")
list = require("Common.Util.list")
require("Common.Util.event")
require("Common.Util.Timer")
UpdateManager = require "Common.UpdateManager"
require "Common.GlobalEventSystem"

require("Game.Common.Action.ActionNodeWrapper")
require("Game.Common.Action.ActionTweenFunction")
require("Game.Common.Action.Action")
require("Game.Common.Action.ActionInterval")
require("Game.Common.Action.ActionEase")
require("Game.Common.Action.ActionInstant")
require("Game.Common.Action.ActionManager")
require("Game.Common.Action.ActionCatmullRom")
-- require("Game.Common.Action.ActionExtend")

--顺序无关的
require("Common.Util.Functor")


require("Tools.CookieWrapper")
