local phones = {}

RegisterServerEvent("MobilePhone:UpdateToggle")
AddEventHandler('MobilePhone:UpdateToggle', function(playerid, toggle)
	phones[tostring(playerid)] = tostring(toggle)
	TriggerClientEvent("MobilePhone:SyncInVeh", -1, phones)
end)

RegisterCommand('call', function(source, args, rawCommand)
	TriggerClientEvent('MobilePhone:Call', source)
end, false)

RegisterCommand('text', function(source, args, rawCommand)
	TriggerClientEvent('MobilePhone:Text', source)
end, false)

function phonesync()
	TriggerClientEvent("MobilePhone:SyncInVeh", -1, phones)
	SetTimeout(20000, phonesync)
end
SetTimeout(1000, phonesync)