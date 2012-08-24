To use the taskbar in normal user mode please run these three commands in a console in
admin or elevated mode

netsh http add urlacl url=http://+:7890/ user={COMPUTERNAME]\{Username}  //is for the client callback
netsh http add urlacl url=http://+:7891/ user={COMPUTERNAME]\{Username}	 //is for manager
netsh http add urlacl url=http://+:7892/ user={COMPUTERNAME]\{Username}	 //is for host