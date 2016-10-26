# CISL

## dependencies
* unity version 5.4.1f1 — https://unity3d.com/get-unity/download/archive
* narrative backend — https://github.com/adebis/NarrativeBackendRPI
* win 10 — VS 2015 — https://go.microsoft.com/fwlink/?LinkId=691978&clcid=0x409
* OSX — Xamarin Studio
  * ≥10.10  — https://www.xamarin.com/download
  * 10.9 — use same installer as above, then replace Xamarin Studio in Applications directory with http://download.xamarin.com/studio/Mac/XamarinStudio-5.9.8.0-0.dmg

## build

* RPI Backend
  * Win 10 — build & run SLN from VS 2015 as administrator "NarrativeBackendRPI/Data Entry/Data Entry/Zeno Data Entry.sln"
  * OSX (TBC, still seem to be getting errors in Unity with this setup)
    * open SLN in Xamarin Studio
    * add NuGet packages for .NET support
      * Microsoft .NET Framework 4 HTTP Client Libraries (2.0.20710.0)
      * Json.NET (4.5.6)
    * build & run
  * start server: query > chat > start server

* Unity
  * open & run "CISL/Assets/Scenes/timelineTest3.unity"

## notes
* press enter to minimize map at start
* buttons and nodes can be used to navigate narrative data
* map scales and zooms to selected data
* press "SHIFT + C + S" to capture screenshots
