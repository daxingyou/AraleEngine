if not LClass then

--code imp class inherit--
function newLuaObject(className,...)
	--print("newLuaObject:"..className);
	local luaTable = load("return "..className)
	if nil==luaTable then print("newLuaObject failed className="..className); end
	return instantiate(luaTable(), ...)
end

function instantiate(a,...)
	if not a then
		error("instantiate table is nil")
		return nil
	end

	local result = {__index=result;}
	setmetatable(result,a)
	result:___new(...)
	return result
end

function createClass(subClassName,subClass,baseClass)
	if  subClass == nil then
		error("createClass: subClass is nil");
	end

	subClass.__index = subClass;
	subClass.___type = subClassName;
	if nil~=baseClass then
		setmetatable(subClass,baseClass);
		subClass[baseClass.___type] = baseClass;
		subClass.___new = function(self,...)
			self[baseClass.___type].___new(self,...);
			--call class func and use object data
			if nil~=subClass.new then subClass.new(self,...); end
		end
	else
		subClass.___new = function(self,...)
			if nil~=subClass.new then subClass.new(self,...); end
		end
	end
	return subClass;
end

end
