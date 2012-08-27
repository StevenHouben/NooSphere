These dll's are part of the openCV support for the Microsoft Pixelsense implementation. These files cannot be installed true nuget and should be added manually to the project. Only the managed dll's (*) can be added as a reference in Visual Studio. The unmanaged dll's(**) should be copied and paste to the bin folder.

(*) Managed dll's (add as reference in project)
------------------
emgu.cv.dll
emgu.cv.ui.dll
emgu.util.dll
zedgraph.dll

(**) Unmanaged dll's (copy to bin folder)
--------------------
All others
