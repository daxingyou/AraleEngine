if not LBattleSceneCtrl then print("same lua, reload ignore!!!") end

local M=
{
	_cs;
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnStart = self.Start
	cs.luaOnDestroy = self.Destroy
end

function M:Start()
	local msg = MsgReqEnterBattle()
    msg.sceneID = 1
    NetMgr.client:sendMsg(Enum.MyMsgId.ReqEnterBattle, msg)
    HeadInfo.Create(CameraMgr.single:GetCamera("MainCamera"))
end

function M:Destroy()
	HeadInfo.Destroy()
end


--=======================
LBattleSceneCtrl = M
createClass("LBattleSceneCtrl",LBattleSceneCtrl)