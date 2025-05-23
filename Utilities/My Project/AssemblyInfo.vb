﻿Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Windows.Markup

' Allgemeine Informationen über eine Assembly werden über die folgenden 
' Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
' die mit einer Assembly verknüpft sind.

' Die Werte der Assemblyattribute überprüfen

<Assembly: AssemblyTitle("Rstyx.Utilities")> 
<Assembly: AssemblyDescription("General Development Utilities")> 
<Assembly: AssemblyCompany("")> 
<Assembly: AssemblyProduct("Rstyx.Utilities")> 
<Assembly: AssemblyCopyright("Copyright © Robert Schwenn 2012-2025")> 
<Assembly: AssemblyTrademark("")> 

' XAML namespace mappings.
<assembly: XmlnsPrefix("http://schemas.rstyx.de/wpf/Utils", "RstyxUtils")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.LoggingConsole")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities.Apps")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities.UI.Binding.Converters")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities.UI.Binding.PushBinding")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities.UI.Controls")>
<assembly: XmlnsDefinition("http://schemas.rstyx.de/wpf/Utils", "Rstyx.Utilities.UI.Resources")>


<Assembly: ComVisible(False)>

'Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
<Assembly: Guid("bdf6a054-5fa7-4018-80c2-13f148ee8da7")> 

' Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
'
'      Hauptversion
'      Nebenversion 
'      Buildnummer
'      Revision
'
' Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern 
' übernehmen, indem Sie "*" eingeben:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("2.19.1.0")> 
<Assembly: AssemblyFileVersion("2.19.1.0")> 
