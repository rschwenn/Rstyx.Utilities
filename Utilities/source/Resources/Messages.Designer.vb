﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Dieser Code wurde von einem Tool generiert.
'     Laufzeitversion:4.0.30319.296
'
'     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
'     der Code erneut generiert wird.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace Resources
    
    'Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    '-Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    'Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    'mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    '''<summary>
    '''  Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Public Class Messages
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Public Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Rstyx.Utilities.Messages", GetType(Messages).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        '''  Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Public Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler beim Start des Excel-GeoTools-Import der Datei &apos;%s&apos; ... ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AppUtils_ErrorInvokingXlGeoToolsImport() As String
            Get
                Return ResourceManager.GetString("AppUtils_ErrorInvokingXlGeoToolsImport", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die CSV-Datei für Excel-GeoTools-Import nicht gefunden. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AppUtils_GeoToolsCsvNotFound() As String
            Get
                Return ResourceManager.GetString("AppUtils_GeoToolsCsvNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Starte Excel-GeoTools-CSV-Import der Datei &apos;%s&apos; ... ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AppUtils_InvokeGeoToolsCsvImport() As String
            Get
                Return ResourceManager.GetString("AppUtils_InvokeGeoToolsCsvImport", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Es ist kein Editor verfügbar. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AppUtils_NoEditorAvailable() As String
            Get
                Return ResourceManager.GetString("AppUtils_NoEditorAvailable", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Import nach Excel nicht möglich, da extrahiertes VB-Skript nicht existiert. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AppUtils_XlmVBScriptNotFound() As String
            Get
                Return ResourceManager.GetString("AppUtils_XlmVBScriptNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Unerwarteter Fehler bei asynchroner Ausführung des Befehls &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_AsyncExecuteFailed() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_AsyncExecuteFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler beim Abbruch des Task. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_ErrorCancellingTask() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_ErrorCancellingTask", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler beim Erzeugen des Abbruch-Befehls. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_ErrorCreatingCancelCommand() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_ErrorCreatingCancelCommand", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei Aufruf der Ereignis-Handler für Befehl &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_ErrorInCalledCanExecuteChangedHandler() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_ErrorInCalledCanExecuteChangedHandler", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Unerwarteter Fehler bei synchroner Ausführung des Befehls &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_SyncExecuteFailed() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_SyncExecuteFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei Anschlussbearbeitung nach Ausführung des Befehls &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property AsyncDelegateUICommand_TaskFinishingFailed() As String
            Get
                Return ResourceManager.GetString("AsyncDelegateUICommand_TaskFinishingFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Datenbank-Verbindung zur Excel-Arbeitsmappe &apos;%s&apos; konnte nicht hergestelt werden. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property DBUtils_ConnectionToExcelWorkbookFailed() As String
            Get
                Return ResourceManager.GetString("DBUtils_ConnectionToExcelWorkbookFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Excel-Arbeitsmappe nicht gefunden. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property DBUtils_ExcelWorkbookNotFound() As String
            Get
                Return ResourceManager.GetString("DBUtils_ExcelWorkbookNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Das Feld &apos;%s&apos; existiert nicht in der Tabelle &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property DBUtils_FieldNotFoundInTable() As String
            Get
                Return ResourceManager.GetString("DBUtils_FieldNotFoundInTable", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Tabelle &apos;%s&apos; aus Excel-Arbeitsmappe &apos;%s&apos; konnte nicht geöffnet werden. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property DBUtils_OpenExcelWorksheetFailed() As String
            Get
                Return ResourceManager.GetString("DBUtils_OpenExcelWorksheetFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei Verarbeitung der Dateien des Ordners &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_ErrorProcessingFolder() As String
            Get
                Return ResourceManager.GetString("FileUtils_ErrorProcessingFolder", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei Verarbeitung der Unterverzeichnisse des Ordners &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_ErrorProcessingSubFolders() As String
            Get
                Return ResourceManager.GetString("FileUtils_ErrorProcessingSubFolders", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Ungültiger Dateiname konnte nicht bereinigt werden: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_ErrorValidatingInvalidFileName() As String
            Get
                Return ResourceManager.GetString("FileUtils_ErrorValidatingInvalidFileName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Ungültiger Dateipfad konnte nicht bereinigt werden: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_ErrorValidatingInvalidFilePath() As String
            Get
                Return ResourceManager.GetString("FileUtils_ErrorValidatingInvalidFilePath", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Aufgerufen mit nicht unterstütztem FilePart-Enum-Wert: &apos;%s&apos; ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_InvalidFilepartEnumValue() As String
            Get
                Return ResourceManager.GetString("FileUtils_InvalidFilepartEnumValue", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Kein existierendes Verzeichnis angegeben. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_NoExistentFolderName() As String
            Get
                Return ResourceManager.GetString("FileUtils_NoExistentFolderName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Keine Dateimaske(n) angegeben. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_NoFileFilter() As String
            Get
                Return ResourceManager.GetString("FileUtils_NoFileFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Keine zu durchsuchenden Verzeichnisse angegeben. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_NoSearchDir() As String
            Get
                Return ResourceManager.GetString("FileUtils_NoSearchDir", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Keinen gültigen Dateifilter angegeben. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_NoValidFileFilter() As String
            Get
                Return ResourceManager.GetString("FileUtils_NoValidFileFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Dateipfad ist zu lang: &apos;%s&apos; ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_PathTooLong() As String
            Get
                Return ResourceManager.GetString("FileUtils_PathTooLong", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Verzeichnis für Dateisuche existiert nicht: &apos;%s&apos; (Arbeitsverzeichnis: &apos;%s&apos;). ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchFolderNotFound() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchFolderNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Leerer Dateifilter für Dateisuche wird ignoriert. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresEmptyFileFilter() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresEmptyFileFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Leeres Verzeichnis für Dateisuche wird ignoriert. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresEmptyFolderName() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresEmptyFolderName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Dateifilter für Dateisuche enthält ungültige Zeichen und wird ignoriert: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresInvalidFileFilter() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresInvalidFileFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Verzeichnis für Dateisuche enthält ungültige Zeichen und wird ignoriert: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresInvalidFolderName() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresInvalidFolderName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Dateifilter für Dateisuche ist wiederholt angegeben und wird ignoriert: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresRepeatedFileFilter() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresRepeatedFileFilter", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Verzeichnis für Dateisuche ist wiederholt angegeben und wird ignoriert: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property FileUtils_SearchIgnoresRepeatedFolderName() As String
            Get
                Return ResourceManager.GetString("FileUtils_SearchIgnoresRepeatedFolderName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Unerwarteter Anwendungsfehler (Application_DispatcherUnhandledException). ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_DispatcherUnhandledException() As String
            Get
                Return ResourceManager.GetString("Global_DispatcherUnhandledException", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei Ausführung von Cinch.ApplicationHelper.DoEvents(). ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_DoEventsFailed() As String
            Get
                Return ResourceManager.GetString("Global_DoEventsFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler in einem aufgerufenen Ereignishandler. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_ErrorFromCalledEventHandler() As String
            Get
                Return ResourceManager.GetString("Global_ErrorFromCalledEventHandler", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler bei der Ereignisbehandlung. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_ErrorFromInsideEventHandler() As String
            Get
                Return ResourceManager.GetString("Global_ErrorFromInsideEventHandler", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die &apos;%s()&apos; wurde noch nicht implementiert! ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_NotImplemented() As String
            Get
                Return ResourceManager.GetString("Global_NotImplemented", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Eigenschaft &apos;%s&apos; ist schreibgeschützt. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_PropertyIsReadOnly() As String
            Get
                Return ResourceManager.GetString("Global_PropertyIsReadOnly", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Eigenschaft &apos;%s&apos; existiert nicht. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_PropertyNotFound() As String
            Get
                Return ResourceManager.GetString("Global_PropertyNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Resource konnte nicht gelesen werden: &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_ResourceNotFound() As String
            Get
                Return ResourceManager.GetString("Global_ResourceNotFound", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Unerwarteter Fehler. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Global_UnexpectedError() As String
            Get
                Return ResourceManager.GetString("Global_UnexpectedError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Nicht erkannte Escape Sequenz: \{0} ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Sprintf_InvalidEscapeSequence() As String
            Get
                Return ResourceManager.GetString("Sprintf_InvalidEscapeSequence", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Nicht erkannte Parameter Sequenz: %{0}{1}{2}{3} ähnelt.
        '''</summary>
        Public Shared ReadOnly Property Sprintf_InvalidParameterSequence() As String
            Get
                Return ResourceManager.GetString("Sprintf_InvalidParameterSequence", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler beim Erstellen des Icon-DrawingBrush &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property UIResources_ErrorCreatingIconDrawingBrush() As String
            Get
                Return ResourceManager.GetString("UIResources_ErrorCreatingIconDrawingBrush", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Sucht eine lokalisierte Zeichenfolge, die Fehler beim Erstellen des Icon-Rechtecks &apos;%s&apos;. ähnelt.
        '''</summary>
        Public Shared ReadOnly Property UIResources_ErrorCreatingIconRectangle() As String
            Get
                Return ResourceManager.GetString("UIResources_ErrorCreatingIconRectangle", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
