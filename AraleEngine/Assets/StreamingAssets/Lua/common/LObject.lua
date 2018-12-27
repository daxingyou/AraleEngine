if not LObject then
--======================
LObject =
{
	a = "a";
	_cso  = nil;
	_mL   = nil;
	_pre  = nil;
	_next = nil;
}

function LObject:new()
	print(self.test);
	print("LObject");
end

function LObject:link(lo)
	assert(self._pre==nil and self._next==nil);
	if nil==lo then
		return self;
	end

	self._next = lo;
	self._pre  = lo._pre;
	if nil~=self._pre then self._pre._next = self; end
	lo._pre    = self;
	return self;--return for head point
end

function LObject:unlink()
	if nil~=self._pre  then self._pre._next = self._next; end
	if nil~=self._next then self._next._pre = self._pre; end
	local np = self._next;
	self._next = nil;
	self._pre  = nil;
	return np;--return for head point
end

_destory = function()
	self:destory();
	self._mL=nil;
end
	

function LObject:init()
end

function LObject:deinit()
end

function LObject:destory()
end

--========================
createClass("LObject",LObject);
end
