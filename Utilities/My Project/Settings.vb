
Namespace My
    
    'Diese Klasse ermöglicht die Behandlung bestimmter Ereignisse der Einstellungsklasse:
    ' Das SettingChanging-Ereignis wird ausgelöst, bevor der Wert einer Einstellung geändert wird.
    ' Das PropertyChanged-Ereignis wird ausgelöst, nachdem der Wert einer Einstellung geändert wurde.
    ' Das SettingsLoaded-Ereignis wird ausgelöst, nachdem die Einstellungswerte geladen wurden.
    ' Das SettingsSaving-Ereignis wird ausgelöst, bevor die Einstellungswerte gespeichert werden.
    Partial Friend NotInheritable Class MySettings
        
        Private Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.GetLogger(MyClass.GetType.FullName)
        
        Private Sub MySettings_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Handles Me.PropertyChanged
            If (My.Settings.Library_AutoSaveSettings) Then
                Logger.LogDebug(StringUtils.Sprintf("PropertyChanged(): Benutzereinstellungen werden gespeichert, da '%s' geändert wurde.", e.PropertyName))
                My.Settings.Save()
            End If
        End Sub
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
