if LBattleSceneCtrl then print("same lua, reload ignore!!!") end

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
    self._onPlayerDie = function (evt) self:OnPlayerDie(evt) end
    EventMgr.single:AddListener("Player.Die", self._onPlayerDie)
end

function M:Destroy()
	EventMgr.single:RemoveListener("Player.Die", self._onPlayerDie)
	HeadInfo.Destroy()
end

function M:OnPlayerDie(evt)
	local imgEffect = Camera.main:GetComponent("ImageEffect")
	if evt.data == true  then
		imgEffect:SetMaterial("Mat/ImageGray")
	else
		imgEffect.mEffectMat=nil
	end
end

--=======================
LBattleSceneCtrl = M
createClass("LBattleSceneCtrl",LBattleSceneCtrl)