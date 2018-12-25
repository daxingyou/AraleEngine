--====================================================
--don't modify the code in this block
--don't modify the lua file in common,permit only System developer
--main.lua is lua code entry
print(package.path);
LuaMono = luanet.import_type("Arale.Engine.LuaMono");
--MonoEvent = luanet.import_type("Arale.Engine.LuaMono.MonoEvent");
require "common/LClass"
require "common/LObject"
require "common/LMono"
print("require all ok");
--=====================your code begin=================
--WHelper = luanet.import_type("WHelper");