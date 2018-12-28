if not LBattleSceneCtrl then
--======================
local M=
{
	_cs;
}

function M:new(cs)
	self._cs = cs;
	cs.luaOnStart = self.Start()
end

function M:Start()
	local msg = MsgReqEnterBattle()
    msg.sceneID = 1
    NetMgr.client:sendMsg(Enum.MyMsgId.ReqEnterBattle, msg)
end


--=======================
LBattleSceneCtrl = M
createClass("LBattleSceneCtrl",LBattleSceneCtrl)
end