' StartExcelAndXLMacro.vbs.vbs  Start Excel and an Excel macro.
'
'
' for jEdit:  :collapseFolds=1:
'

Option explicit
on error goto 0

const Version     = "v2.7b"

' --- Klassen einbinden ----------------------------------------------
  dim oSkript, oXLTools, oTools_1
  
  set oSkript = new Skript
  set oTools_1 = new Tools_1
  set oXLTools = new XLTools
' --- Ende Include -----------------------------------------------------------

'Deklarationen:
'Objekte
  Dim WSHShell, fs, xl
  
'Strings
  Dim Aufruf, Makro, MParam, Datei
  dim i, ExcelArbVerz, ExcelNeuGestartet
  
'Objekte instanzieren
  Aufruf = "Aufruf: (W|C)script.exe Excel_Makro.vbs /M:<Excel-Makro>" & vbNewline &_
           "                                       [/D:<Dateiname>]" & vbNewline &_
           "                                       [/silent:true|false]" & vbNewline &_
           "                                       [/debug:true|false]"
 
  set fs       = CreateObject("Scripting.FileSystemObject")
  set WSHShell = CreateObject("WScript.Shell")
  
'Globaler Parameter ==> Änderung des Standards.
  if not oSkript.oArgsNamed.exists("silent")  then oSkript.SilentMode = true
  
'Kommandozeile auswerten.
  if oSkript.oArgsNamed.exists("m") then
    Makro = oSkript.oArgsNamed.item("m")
    call oSkript.DeleteNamedArg("m")
  end if
  if oSkript.oArgsNamed.exists("d") then
    MParam = oSkript.oArgsNamed.item("d")
    call oSkript.DeleteNamedArg("d")
  end if
  
  oSkript.Echo "----------------------------------------------------------"
  oSkript.Echo " Start eines Excel-Makros                            " & Version
  oSkript.Echo "----------------------------------------------------------" & vbNewLine
  oSkript.DebugEcho "SkriptHost:         '" & oSkript.SkripthostName & "'"
  oSkript.Echo      "übergeb. Parameter: '" & oSkript.ArgZeile & "'"
  oSkript.Echo      "Excel-Makro:        '" & Makro & "'"
  oSkript.Echo      "Makro-Parameter:    '" & MParam & "'"
  
'Datei immer mit Pfad an Excel übergeben.
  if (MParam <> "") then Datei = fs.GetAbsolutePathName(MParam) else Datei = ""
  
'Excel starten, wenn möglich
  if (Makro = "") then
    'Makro-Parameter darf "-" sein, aber nicht fehlen.
    oSkript.EchoStop "FEHLER: kein Excel-Makro angegeben! " & vbnewline & vbnewline & Aufruf
    
  elseif ((Makro = "-") and not fs.FileExists(Datei)) then
    'Wenn Makro-Parameter = "-", dann soll nur "Datei" geöffnet werden
    oSkript.EchoStop "FEHLER: Zu öffnende Datei '" & Datei & "' existiert nicht!" & vbnewline & vbnewline & Aufruf
    
  else
    'Makro soll ausgeführt werden. Datei ist optional.
    
    if ((Datei <> "") and (not fs.FileExists(Datei))) then
    'if (not fs.FileExists(Datei)) then
      oSkript.EchoStop "FEHLER: angegebene Datei '" & Datei & "' existiert nicht!" & vbnewline & vbnewline & Aufruf
    else
      'Datei-Parameter i.O. => Excel finden bzw. starten
      set xl = oXLTools.GetExcelApp(false)
      if (xl is nothing) then
        oSkript.EchoStop "Excel konnte nicht gefunden oder gestartet werden!"
        oSkript.AbbruchSkript
      else
        'Excel gefunden oder gestartet.
        
        if (Makro = "-") then
          'kein Makro ausführen, sonder Datei öffnen.
          oSkript.echo "Datei '" & Datei & "' wird geöffnet."
          on error resume next
          xl.Workbooks.open Datei
          if (err.number = 0) then 
            oSkript.Echo "Datei '" & Datei & "' in Excel geöffnet."
            oXLTools.SetExcelAppSichtbarkeit(true)
          else
            oSkript.ErrEcho ""
            oSkript.EchoStop "FEHLER bei Öffnen der Datei '" & Datei & "' in Excel."
            on error goto 0
            if (oXLTools.ExcelNeuGestartet) then
              xl.Quit
            else
              oXLTools.SetExcelAppSichtbarkeit(true)
            end if
          end if
          
        else
          'Makro ausführen
          oSkript.echo "Makro '" & Makro & "' wird gestartet."
          on error resume next
          if (Datei <> "") then
            xl.run Makro, Datei
          else
            oXLTools.SetExcelAppSichtbarkeit(true)
            xl.run Makro
          end if
          if (err.number = 0) then 
            oSkript.Echo "Excel-Makro '" & Makro & "' fehlerfrei beendet."
            oSkript.EchoPause "Excel-Makro '" & Makro & "' fehlerfrei beendet."
            oXLTools.SetExcelAppSichtbarkeit(true)
          else
            if (err.number = 1004) then
              oSkript.ErrEcho ""
              oSkript.EchoStop "FEHLER: Excel-Makro '" & Makro & "' kann nicht gefunden werden!"
            else 
              oSkript.ErrEcho ""
              oSkript.EchoStop "FEHLER bei Ausführung des Excel-Makro '" & Makro & "'."
            end if
            on error goto 0
            if (oXLTools.ExcelNeuGestartet) then
              xl.Quit
            else
              oXLTools.SetExcelAppSichtbarkeit(true)
            end if
          end if
          
        end if
        set xl = nothing
        
      end if
    end if
  end if
  
  'oSkript.AnzeigeMeldungen
  
'Aufräumen
  set fs         = nothing
  set WSHShell   = nothing
  
' --- Global.vbi --------------------------------------------------------
'Konstanten (Diese lassen sich nicht in Klassen festlegen.)
 'Zentrales Include-Verzeichnis
  const strIncludeVerzZentral = "R:\Windows\WScripts"
  
 'WshShell.Run
  const WindowStyle_hidden     = 0
  const WindowStyle_normal     = 1
  const WindowStyle_minimized  = 2
  const WindowStyle_maximized  = 3
  const WaitOnReturn_yes       = true
  const WaitOnReturn_no        = false
  
 'Dateioperationen
  const NewFileIfNotExist_yes  = true
  const NewFileIfNotExist_no   = false
  const OpenAsASCII            = -0
  const OpenAsUnicode          = -1
  const OpenAsSystemDefault    = -2
  
  const ForReading             = 1
  const ForWriting             = 2
  const ForAppending           = 8
  
  const TristateFalse          = -0
  const TristateTrue           = -1
  const TristateUseDefault     = -2
  
 'FileSystemObject.GetSpecialFolder
  const WindowsOrdner          = 0
  const SystemOrdner           = 1
  const TempOrdner             = 2
  
 'Reguläre Ausdrücke
  const IgnoreCase_Yes         = true
  const IgnoreCase_No          = false
  
 'String-Operationen
  'const vbBinaryCompare       = 0  'Konstante ist vorhanden.
  'const vbTextCompare         = 1  'Konstante ist vorhanden.
                               
 'Datenbank
  const adModeRead             = 1
  const adModeReadWrite        = 3
  const adStateClosed          = 0
  const adStateOpen            = 1
  const adStateConnecting      = 2
  
 'Excel
  const xlDown                 = -4121
  const xlUp                   = -4162
  const xlNormal               = -4143   'FensterStatus Excel
  const xlMaximized            = -4137   'FensterStatus Excel
  const xlPasteValues          = -4163
  
 'wshCommonDialogs.ocx
  Const OFN_READONLY           = &H1
  Const OFN_OVERWRITEPROMPT    = &H2
  Const OFN_HIDEREADONLY       = &H4
  Const OFN_NOCHANGEDIR        = &H8
  Const OFN_NOVALIDATE         = &H100
  Const OFN_PATHMUSTEXIST      = &H800
  Const OFN_FILEMUSTEXIST      = &H1000
  Const OFN_CREATEPROMPT       = &H2000
  Const OFN_SHAREAWARE         = &H4000
  Const OFN_NOREADONLYRETURN   = &H8000
  Const OFN_NOTESTFILECREATE   = &H10000
  Const OFN_NONETWORKBUTTON    = &H20000
  Const OFN_NOLONGNAMES        = &H40000
  Const OFN_EXPLORER           = &H80000
  Const OFN_NODEREFERENCELINKS = &H100000
  Const OFN_LONGNAMES          = &H200000
'
class Skript
  '- Informationen zum Skript und dessen Umgebung.
  '- Host-unabhängige Aktionen: Meldungsausgabe, Unterbrechung, Wartezeit, Skriptabbruch.
  '- RunGetAusgabe(): Ausführen eines Kommandos, dessen Ausgaben abgefangen werden.
  '
  '==> Läuft das aufrufende Skript im Browser (IE), so müssen dort folgende globale
  '    Variablen definiert sein: "app" als HTA-Anwendungs-ID.
  

  '***  Deklarationen  ****************************************************************************.
  public  OS_Typ, OS_Name, OS_VersionsNr
  public  OS_SystemRoot, OS_WindowsSys, OS_SystemDrive, OS_Comspec
  public  OSDir_Programme
  public  Var_TEMP
  public  SkripthostName, SkriptName, SkriptPfadName
  public  oArgsNamed, oArgsUnnamed, oArgsDateinamen, oArgsDateimasken, oArgsSonstige
  public  ArgZeile, ArgZeileUnnamed, ArgZeileUnnamed2, ArgZeileUnnamed3, ArgZeileNamed, ArgZeileNamed2
  public  ArgZeileDateinamen, ArgZeileDateinamen2, ArgZeileDateimasken, ArgZeileDateimasken2
  public  ArgZeileSonstige, ArgZeileSonstige2, ArgZeileUnnamed4
  public  DebugMode, SilentMode
  public  LogQueue, ErrQueue, DebugQueue
  public  ListenTrenner, DezimalTrenner, LangID
  
  private WshShell, WshEnv, fs, oArgs, oMeldungen, oRegExp
  private SleepVbs, PrefixNamedArg


  
  '===  Variablen-Belegung  ========================================================================
 
  private Sub Class_Initialize()
    
    dim Arg
    
    PrefixNamedArg = "/"        'Erkennung eines benannten Kommandozeilenparameters.
    
    set WshShell   = CreateObject("WScript.Shell")
    'set WshEnv     = WshShell.Environment("Process") 
    set fs         = CreateObject("Scripting.FileSystemObject")
    set oMeldungen = CreateObject("Scripting.Dictionary")
    Set oRegExp    = New RegExp
    
    'Standardwerte.
    SilentMode = false
    DebugMode  = false

    SkripthostName = GetSkripthostName()
    call CreateSleepVbs()
    call GetOS()
    call GetSystemUmgebung()
    call GetInternational()
    call GetSkriptNamen()
    call GetKommandozeile()

    if oArgsNamed.exists("debug")  then
      DebugMode = lcase(oArgsNamed.item("debug"))
      if (DebugMode <> "false") then DebugMode = true
      call DeleteNamedArg("debug")
    end if
    if oArgsNamed.exists("silent") then
      SilentMode = lcase(oArgsNamed.item("silent"))
      if (SilentMode <> "false") then SilentMode = true
      call DeleteNamedArg("silent")
    end if
    if DebugMode then SilentMode = false
  
    debugecho "Klasse 'Skript' erfolgreich instanziert für Skript '" & SkriptName & "'  ('" & SkriptPfadName & "')."
    debugecho "Skripthost = '" & SkripthostName &  "',      Arbeitsverzeichnis = '" & ArbeitsVerz & "'."
    debugecho "Betriebssystem:  Typ='" & OS_Typ & "'  Name='" & OS_Name & "'  Version ='" & OS_VersionsNr & "'."
    debugecho "Windows Laufwerk='" & OS_SystemDrive & "'  Stammverz.='" & OS_SystemRoot & "'  Systemverz.='" & OS_WindowsSys & "'  Kommandoprozessor='" & OS_Comspec & "'."
    debugecho "Language ID='" & LangID & "'  ListenTrenner='" & ListenTrenner & "'  DezimalTrenner='" & DezimalTrenner & "'."
    debugecho "Argumente = '" & ArgZeile & "'"
    debugecho "Anzahl Argumente=" & oArgsUnnamed.count + oArgsNamed.count & ", davon: unbenannt=" & oArgsUnnamed.count & ",  benannt=" & oArgsNamed.count
    For each Arg in oArgsUnnamed
      debugecho  "Unbenannter Parameter: " & oArgsUnnamed(Arg)
    Next
    For each Arg in oArgsNamed
      debugecho  "Benannter Parameter: " & Arg & " = " & oArgsNamed(Arg)
    Next
    debugecho "ArgZeileNamed2 = '"       & ArgZeileNamed2 & "'"
    debugecho "ArgZeileUnnamed2 = '"     & ArgZeileUnnamed2 & "'"
    debugecho "ArgZeileUnnamed3 = '"     & ArgZeileUnnamed3 & "'"
    debugecho "ArgZeileUnnamed4 = '"     & ArgZeileUnnamed4 & "'"
    debugecho "ArgZeileDateimasken2 = '" & ArgZeileDateimasken2 & "'"
    debugecho "ArgZeileDateinamen2 = '"  & ArgZeileDateinamen2 & "'"
    debugecho "ArgZeileSonstige2 = '"    & ArgZeileSonstige2 & "'"
  end sub

  
 
  private Sub Class_Terminate()
    'Im DebugModus Anzeige aller restlichen Meldungen erzwingen.
    if (DebugMode) then call AnzeigeMeldungen
    set WshShell         = nothing
    'set WshEnv           = nothing
    set fs               = nothing
    set oRegExp          = Nothing
    set oArgs            = nothing
    set oArgsNamed       = nothing
    set oArgsUnnamed     = nothing
    set oArgsDateinamen  = nothing
    set oArgsDateimasken = nothing
    set oArgsSonstige    = nothing
    debugecho "Klasse 'Skript' beendet."
  end sub
 

 
  '===  Eigenschaften  ============================================================================


  public property get ArbeitsVerz()
    ArbeitsVerz = WshShell.CurrentDirectory
  end property

  public property let ArbeitsVerz(inpVerz)
    if (fs.FolderExists(inpVerz)) then
      WshShell.CurrentDirectory = inpVerz
      debugecho "Arbeitsverzeichnis gesetzt auf '" & inpVerz & "'."
    else
      debugecho "Arbeitsverzeichnis setzen: Verzeichnis '" & inpVerz & "' existiert nicht."
    end if
  end property
 

 
  '===  Methoden  =================================================================================


  public sub AbbruchSkript()
    'Host-unabhängiger Abbruch der Skriptverarbeitung (Im IE gibt's das WScript-Objekt nicht).
    'Bei IE als Host wird das IE-Fenster geschlossen (d.h. z.B. das HTA beendet).
    on error resume next
    'WSH als Host.
    wscript.quit
    if(err.number <> 0) then 
      'IE als Host.
      err.clear
      window.close
      'Die folgende Messagebox wird nicht angezeigt. Aber ohne diese Zeile bricht ein
      'in HTML eingebettes Skript nicht (sicher) ab.
      msgbox "dummydummydummydummy"
    end if
  end sub



  public sub Echo(Message)
    'Gibt "Message" abhängig vom Host als Meldung aus: 
    '  Cscript: sofort auf Konsole.
    '  Wscript: in Dictionary "oMeldungen".
    '  IE:      in "LogQueue" und "DebugQueue".
    dim Erfolg
    
    if (SkripthostName = "cscript.exe") then
      wscript.echo SkriptName & ": " & Message
    
    elseif (SkripthostName = "wscript.exe") then
      oMeldungen.Add oMeldungen.count, Message
      
    elseif (SkripthostName = "IE") then
      'Versuch, die Meldung in die HTA-Queues zu stellen. 
      'Falls oHTA (noch) nicht existiert, erstmal hier sammeln.
      on error resume next
      Erfolg = false
      Erfolg = oHTA.addQueueMessage("LogQueue", Message)
      on error goto 0
      if (not Erfolg) then LogQueue = LogQueue & Message & vbNewLine
      
      on error resume next
      Erfolg = false
      Erfolg = oHTA.addQueueMessage("DebugQueue", Message)
      on error goto 0
      if (not Erfolg) then DebugQueue = DebugQueue & Message & vbNewLine
    end if
    
  End sub



  public sub DebugEcho(Message)
    'Gibt "Message" abhängig vom Host als Meldung aus, wenn DebugMode=true:
    '  Cscript - wenn DebugMode=true: sofort auf Konsole.
    '  Wscript - wenn DebugMode=true: in Dictionary "oMeldungen".
    '  IE      - immer              : in "DebugQueue".
    dim Erfolg
    
    if (SkripthostName = "IE") then
      'Versuch, die Meldung in die HTA-Queue zu stellen. 
      'Falls oHTA (noch) nicht existiert, erstmal hier sammeln.
      on error resume next
      Erfolg = false
      Erfolg = oHTA.addQueueMessage("DebugQueue", Message)
      on error goto 0
      if (not Erfolg) then DebugQueue = DebugQueue & Message & vbNewLine
      
    elseif (DebugMode) then
      if (SkripthostName = "cscript.exe") then
        wscript.echo SkriptName & ": " & Message
      elseif (SkripthostName = "wscript.exe") then
        oMeldungen.Add oMeldungen.count, Message
      end if
    end if

  End sub


  public sub ErrEcho(Message)
    'Gibt "Message" abhängig vom Host als Meldung aus: 
    '  Cscript: sofort auf Konsole.
    '  Wscript: in Dictionary "oMeldungen".
    '  IE:      in "LogQueue", "ErrQueue" und "DebugQueue".
    'Falls Err.Number <> 0, dann werden folgende Informationen zu diesem Fehler 
    'angezeigt: Quelle, Nummer, Beschreibung. Danach wird der Fehler gelöscht.
    Dim ErrInfo, Erfolg
    
    if (err <> 0) then
      ErrInfo = "FEHLER in: '" & Err.Source & "':" & vbNewLine & _
                "Fehlernummer      : " & Err.Number & vbNewLine & _
                "Fehlerbeschreibung: " & Err.Description & vbNewLine
      Err.Clear
    end if
    
    if (SkripthostName = "IE") then
      'Versuch, die Meldung in die HTA-Queues zu stellen. 
      'Falls oHTA (noch) nicht existiert, erstmal hier sammeln.
      on error resume next
      Erfolg = false
      Erfolg = oHTA.addQueueMessage("ErrQueue", ErrInfo & Message)
      on error goto 0
      if (not Erfolg) then ErrQueue = ErrQueue & ErrInfo & Message & vbNewLine
    end if
    
    call echo(ErrInfo & Message)
    
  End sub
  
  
  
  public sub EchoPause(Message)
    'Skriptunterbrechung mit Ausgabe von "Message" als Meldung.
    '==> Wirkungslos, wenn SilentMode=true.
    '==> Wirkungslos, wenn SkripthostName="IE" (wie sollte eine Pause aussehen?).
    'Wenn Wscript läuft: Anzeige der bisher gesammelten Meldungen sowie "Message"
    '                    in einem Dialogfenster
    'Wenn Cscript läuft: Ausgabe von "Message" auf der Konsole sowie in einem modalen
    '                    Dialogfenster. Damit ist sichergestellt, das die Meldung und
    '                    damit die Eingabeaufforderung im Vordergrund steht. Solange
    '                    der Dialog nicht bestätigt wurde, sollte auch das Konsolen-
    '                    fenster noch vorhanden und somit lesbar sein.
    
    if (SkripthostName <> "IE") then
      if (not SilentMode) then
        if (SkripthostName = "cscript.exe") then
          echo Message
        end if
        oMeldungen.Add oMeldungen.count, ""
        oMeldungen.Add oMeldungen.count, Message
        call AnzeigeMeldungen
      end if
    end if
  
  End sub



  public sub EchoStop(Message)
    'Bekanntgabe des Programmabbruchs wegen Fehler.
    'Gibt "Message" abhängig vom Host als Meldung aus: 
    'Wscript+IE: Anzeige der bisher gesammelten Meldungen sowie "Message" in einem Dialogfenster.
    'Cscript:    Ausgabe von "Message" auf der Konsole sowie in einem modalen Dialogfenster.
    '            Damit ist sichergestellt, das die Meldung und damit die Eingabeaufforderung
    '            im Vordergrund steht. Solange der Dialog nicht bestätigt wurde, sollte
    '            auch das Konsolenfenster noch vorhanden und somit lesbar sein.
    ' ==> Die Meldung über den Abbruch der Skriptbearbeitung wird automatisch angefügt.
    '
    ' ==> Die Skriptausführung wird NICHT automatisch beendet.
  
    dim AbbruchMeldung
    AbbruchMeldung = "***********  Abbruch!  ********************************"
    
    if (SkripthostName = "cscript.exe") then
      echo Message
      echo AbbruchMeldung
    end if
    oMeldungen.Add oMeldungen.count, ""
    oMeldungen.Add oMeldungen.count, Message
    oMeldungen.Add oMeldungen.count, ""
    oMeldungen.Add oMeldungen.count, AbbruchMeldung
    call AnzeigeMeldungen

  End sub



  public sub AnzeigeMeldungen()
    'Gibt alle bisher durch echo() und debugecho() gesammelten Skript-Meldungen 
    'in Blöcken zu je xx Zeilen aus und löscht diese dann.
    '=> Wirkungslos, wenn SkripthostName <> "Wscript.exe", da dann keine Meldungen gesammelt werden.
    Dim MeldungenItems, MeldungenKeys, msg, Rest, i, Titel
    
    const BlockLaenge = 20    ' Anzahl Zeilen, die gemeinsam im Dialogfenster ausgegeben werden
  
    Titel = SkriptName
    if (DebugMode) then Titel = Titel & "  (Debugmodus)"
    if (oMeldungen.Count>0) then
      MeldungenItems = oMeldungen.Items
      MeldungenKeys  = oMeldungen.Keys
      msg            = ""
      For i = 1 To oMeldungen.Count
        Rest = i mod BlockLaenge
        ' msgbox "i=" & i & "     Rest=" & Rest
        if Rest = 0 then 
          WSHShell.popup msg, 0, Titel, vbInformation
          msg = ""
        end if
        msg = msg & MeldungenItems(i-1) & vbNewLine
      Next
      WSHShell.popup msg, 0, Titel, vbInformation
      oMeldungen.RemoveAll
    end if
  End sub


  public sub LoescheMeldungen()
    'Löscht alle bisher durch echo() und debugecho() gesammelten Skript-Meldungen,
    'wenn nicht gerade der Debug-Modus aktiv ist.
    if (not DebugMode) then oMeldungen.RemoveAll
  End sub


  
  public Sub Wait(Millisec)
    'Wartet die angegebene Anzahl Millisekunden. Host-unabhängig, beansprucht ggf. CPU-Ressourcen.
    if ((SkripthostName = "cscript.exe") or (SkripthostName = "wscript.exe")) then
      WScript.Sleep Millisec
    else
      'msgbox "wscript.exe " & SleepVbs & " " & Millisec
      WshShell.Run "wscript.exe " & SleepVbs & " " & Millisec, , WaitOnReturn_yes
    end if
    'Alte Notvariante:
    'dim Start, i
    'Start = cint(second(time))
    'i = 0
    'Do
    '  i = i + 1
    'loop While cint(second(time)) < Start + cint(x)
  End Sub


  sub StatusBusyAnzeige()
    'Falls oHTA existiert, wird die Beschäftigungsanzeige - falls initialisiert - aufgefrischt.
    on error resume next
    oHTA.StatusBusyAnzeige()
    on error goto 0
  end sub
  
  

  public Function RunGetAusgabe(byVal Kommando, byVal TimeOut)
    'Führt das angegebene Kommando aus, leitet dessen Ausgaben in eine temporäre
    'Datei um, liest diese ein und gibt sie als Funktionswert zurück.
    'Parameter: Kommando  ... Befehl: - darf keine Ausgabeumleitung enthalten
    '                                 - sollte immer mit %comspec /c " beginnen (sonst Probleme!!!)
    '           TimeOut   ... Zeit in Sekunden, nach der nicht weiter auf das 
    '                         Prozessende gewartet wird (Notbremse gegen ungeschickte Kommandos o.ä.).
    'Rückgabe:  String mit der kompletten Standardausgabe des Kommandos
    '           oder "@@@_FEHLER_@@@"
    
    Dim k, i, Ausgabe, KommandoMitUmleitung
    Dim Beginn_Exec, Dauer, blnPause, TmpDateiPfadName, DateiAnzZeilen
    Dim oExec, oDateiTmp, oDateiTmpStream, Status
    Dim otmpVerz, tmpDatName
    
    Ausgabe          = ""
    'Set otmpVerz     = fs.GetSpecialFolder(TempOrdner)
    tmpDatName       = fs.GetTempName
    'TmpDateiPfadName = otmpVerz.drive & "\" & otmpVerz.name & "\" & tmpDatName
    TmpDateiPfadName = fs.GetSpecialFolder(TempOrdner) & "\" & tmpDatName
    
    '******  TEST TEST TEST TEST TEST 
    'if (left(lcase(Kommando), 4) <> "start") then
    '  if (OS_Name = "WXP") then
    '    Kommando = "start /min " & Kommando
    '  end if
    'end if
    
    KommandoMitUmleitung = Kommando & " > " & TmpDateiPfadName
    debugecho vbNewLine & "RunGetAusgabe() startet den Befehl '" & KommandoMitUmleitung & "'."
    
    'Prozess starten.
    on error resume next
    '----------------------------------------------------------------------------------------------
    Set oExec = WshShell.Exec(KommandoMitUmleitung)
    '----------------------------------------------------------------------------------------------
    'Ergebnis dieser Anweisung, wenn das Kommando NICHT ausgeführt werden konnte:
    ' WinXP: - Zuweisung schlägt fehl, d.h. oExec ist KEIN Objekt!
    '        - Ein Fehler tritt auf, d.h. err.number <> 0
    ' Win98: - Zuweisung erfolgreich, d.h. oExec ist immer ein Objekt
    '        - oExec.Status =    1
    '        - oExec.ExitCode =  0
    '        - oExec.ProcessID = 0  *** Dies scheint der entscheidende Hinweis zu sein! ***
    '        - Es tritt KEIN Fehler auf, d.h. err.number = 0
    '
    'Ergebnis dieser Anweisung, wenn das Kommando ausgeführt werden konnte:
    ' Win98+XP: - Zuweisung erfolgreich, d.h. oExec ist ein Objekt
    '           - oExec.Status =    1, wenn fertig;  0, wenn noch beschäftigt
    '           - oExec.ExitCode =  0, wenn erfolgreich oder oExec.Status = 0, sonst ein programmabhängiger Kode.
    '           - oExec.ProcessID <> 0 
    '----------------------------------------------------------------------------------------------
    
    if (isObject(oExec)) then
      if (oExec.ProcessID = 0) then
        'Win98: XP-Verhalten simulieren.
        err.source = "WshShell.Exec"
        err.description = "Der Prozess wurde nicht erzeugt (z.B. Programm nicht gefunden...)"
        err.raise -2147024894
      end if
    end if
    
    if (err <> 0) then
      ErrEcho "RunGetAusgabe(): Der Befehl '" & Kommando & "' konnte nicht ausgeführt werden."
      on error goto 0
      Ausgabe = "@@@_FEHLER_@@@"
    else
      'Warten auf Prozessende bzw. Abbruch nach Timeout.
      on error goto 0
      Beginn_Exec = Timer
      Dauer       = 0
      Do
        debugecho "### Schleife: warten auf Prozessende"
        wait 100
        Dauer = Timer - Beginn_Exec
      Loop Until ((oExec.Status <> 0) or (Dauer > TimeOut))
      
      debugecho "oExec.Status = '" & oExec.Status & "'"
      debugecho "oExec.ExitCode = '" & oExec.ExitCode & "'"
      debugecho "oExec.ProcessID = '" & oExec.ProcessID & "'"
      
      Status = oExec.Status
      if (Status = 0) then 
        debugecho "Timeout (" & TimeOut & " sec) überschritten ==> Prozess wird abgebrochen."
        oExec.Terminate   '=> Damit wird auch die temporäre Datei geschlossen.
                          '=> oExec.Status und oExec.ExitCode beiben unverändert!
      else
        if (oExec.ExitCode = 0) then
          debugecho "Prozess regulär beendet."
        else
          debugecho "Prozess mit FEHLER beendet (ExitCode = " & oExec.ExitCode & ")."
        end if
      end if
      
      'Ausgaben des Programmes einlesen.
      if fs.FileExists(TmpDateiPfadName) then
        Set oDateiTmp = fs.GetFile(TmpDateiPfadName)
        if (oDateiTmp.size > 0) then
          debugecho "Lese temporäre Datei '" & TmpDateiPfadName & "'."
          Set oDateiTmpStream = oDateiTmp.OpenAsTextStream(ForReading, OpenAsSystemDefault)
          Ausgabe = oDateiTmpStream.ReadAll
          oDateiTmpStream.close
          on error resume next
          oDateiTmp.delete
          on error goto 0
          debugecho "RunGetAusgabe(): Ausgaben des Befehls '" & Kommando & "' :" & vbNewLine & Ausgabe
        else
          debugecho "RunGetAusgabe(): Der Befehl '" & Kommando & "' hat nichts ausgegeben."
        end if
      else
        debugecho "Temporäre Datei '" & TmpDateiPfadName & "' existiert nicht."
      end if
    end if
    
    Set oExec = Nothing
    'Set otmpVerz = Nothing
    Set oDateiTmp = Nothing
    Set oDateiTmpStream = Nothing
    
    RunGetAusgabe = Ausgabe
  end function
  
  
  
  '===  interne Routinen  =========================================================================
  
  
  private sub GetOS()
    'Ermittelt Informationen zur Version des Betriebssystems.
    'Es werden folgende Variablen belegt:
    '  OS_Typ          ... Betriebssystem Typ ("Windows_9x", "Windows_NT")
    '  OS_Name         ... Betriebssystem Name ("W95", "W98", "WME", "WNT", "W2K", "WXP", "WVista", "W7", "W8", "W8.1")
    '  OS_VersionsNr   ... Betriebssystem Versionsnummer komplett 
    '
    'Ursprüngliche Routinen: Torgeir Bakken (M. Harris' Favorit.)
  
    Dim sResults, oMatches, VersionsNr, NrGefunden
  
    'Standardwerte
    OS_Typ        = "?"
    OS_Name       = "?"
    OS_VersionsNr = "?"
  
    'Typ ermitteln (auf einigermaßen sichere Art).
    if WSHShell.ExpandEnvironmentStrings("%Systemroot%")="%Systemroot%" then
      OS_Typ = "Windows_9x"
    else
      OS_Typ = "Windows_NT"
    end if                         
  
    '"ver"-Kommando absetzen und dessen Ausgabe entgegennehmen.
    sResults = RunGetAusgabe("%comspec% /c ver", 2)
    
    if (sResults <> "") then
      'Versionsnummer ermitteln.
      NrGefunden = false
      oRegExp.Pattern    = "Version ([0-9.]+)"
      oRegExp.IgnoreCase = true 
      oRegExp.Global     = false         
      set oMatches       = oRegExp.Execute(sResults)
      if (oMatches.Count > 0) then
        if (oMatches(0).SubMatches.Count > 0) then
          VersionsNr = oMatches(0).SubMatches(0)
          NrGefunden = true
        end if
      end if
      if (NrGefunden) then
        OS_VersionsNr = VersionsNr
      end if
      
      Select Case True
        Case InStr(sResults, "Windows 95") > 1         : OS_Name = "W95"
        Case InStr(sResults, "Windows 98") > 1         : OS_Name = "W98"
        Case InStr(sResults, "Windows Millennium") > 1 : OS_Name = "WME"
        Case InStr(sResults, "Windows NT") > 1         : OS_Name = "WNT"
        Case InStr(sResults, "Windows 2000") > 1       : OS_Name = "W2k"
        Case InStr(sResults, "Windows XP") > 1         : OS_Name = "WXP"
        Case InStr(sResults, "Version 6.0") > 1        : OS_Name = "WVista"
        Case InStr(sResults, "Version 6.1") > 1        : OS_Name = "W7"
        Case InStr(sResults, "Version 6.2") > 1        : OS_Name = "W8"
        Case InStr(sResults, "Version 6.3") > 1        : OS_Name = "W8.1"
      End Select
  
    end if
  
    Set oMatches  = nothing
  
  End sub
  
  
  
  private sub GetSystemUmgebung()
    'Ermittelt Umgebung des Betriebssystems:
    ' - Systemverzeichnisse
    ' - Kommandoprozessor
    ' - Variablen
  
    const Key_ProgramFilesDir = "HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\ProgramFilesDir"
    
    'Standards.
    OS_SystemRoot   = "?"
    OS_WindowsSys   = "?"
    OS_SystemDrive  = "?"
    OSDir_Programme = "?"
  
    'Systemverzeichnisse.
    if (OS_Typ = "Windows_9x") then
      OS_SystemRoot  = WSHShell.ExpandEnvironmentStrings("%WinDir%")
      OS_WindowsSys  = OS_SystemRoot & "\System"
      OS_SystemDrive = left(OS_SystemRoot,2)
    elseif (OS_Typ = "Windows_NT") then
      OS_SystemRoot  = WSHShell.ExpandEnvironmentStrings("%SystemRoot%")
      OS_WindowsSys  = OS_SystemRoot & "\System32"
      OS_SystemDrive = WSHShell.ExpandEnvironmentStrings("%SystemDrive%")
    end if
  
    if (RegValueExists(Key_ProgramFilesDir)) then
      OSDir_Programme = WSHShell.regread(Key_ProgramFilesDir)
    end if
  
  
    'Kommandoprozessor.
    OS_Comspec = WSHShell.ExpandEnvironmentStrings("%comspec%")
    if (OS_Comspec = "") then OS_Comspec = "?"
    
    
    'Variablen
    Var_TEMP = WSHShell.ExpandEnvironmentStrings("%TEMP%")
    if (right(var_TEMP, 1)) = "\" then var_TEMP = left(var_TEMP, len(var_TEMP) - 1)
  
  End sub
  
  
  
  private sub CreateSleepVbs()
    'Erzeugt ein VB-Skript, das von wait() benötigt wird.
    dim oTS, TEMP
    TEMP = WSHShell.ExpandEnvironmentStrings("%TEMP%")
    if (right(TEMP, 1)) = "\" then TEMP = left(TEMP, len(TEMP) - 1)
    if (fs.FolderExists(TEMP)) then
      SleepVbs = TEMP & "\" & "sleep.vbs"
    else
      SleepVbs = OS_SystemDrive & "\" & "sleep.vbs"
    end if
    Set oTS = fs.CreateTextFile(SleepVbs,1)
    oTS.WriteLine "if (wscript.arguments.Unnamed.Count > 0) then Millisec = wscript.arguments.Unnamed(0) else Millisec = 10"
    oTS.WriteLine "WScript.Sleep Millisec"
    Set oTS = Nothing
  End sub
  
  
  
  private Function GetSkripthostName()
    ' Funktionswert = Name des Skripthost ("cscript.exe", "wscript.exe" oder "IE")
    Dim SkripthostPfadName, Hostname, Browser
    on error resume next
    set SkripthostPfadName = fs.GetFile(WScript.fullName)
    if(err.number = 0) then
      Hostname = lcase(SkripthostPfadName.Name)
    else
      err.clear
      Browser = window.clientInformation.AppName
      if(err.number = 0) then
        if (Browser = "Microsoft Internet Explorer") then
          Hostname = "IE"
        else
          Hostname = "Browser unbestimmt"
        end if
      else
        err.clear
        Hostname = "?"
      end if
    end if
    on error goto 0
    set SkripthostPfadName = nothing
    GetSkripthostName = Hostname
  End Function
  
  
  
  private sub GetSkriptNamen()
    'Ermittelt die Namen des Skriptes bzw. der HTA-Anwendung.
    SkriptName     = "?"
    SkriptPfadName = "?"
    if ((SkripthostName = "cscript.exe") or (SkripthostName = "wscript.exe")) then
      SkriptName = WScript.ScriptName
      SkriptPfadName = WScript.ScriptFullName
    elseif (SkripthostName = "IE") then
      SkriptName = app.ApplicationName
      'SkriptPfadName wird beim Auswerten der Kommandozeile ermittelt.
    end if
  End sub
  
  
  
  private Sub GetKommandozeile()
    'Ermittlung der Kommandozeilen-Parameter.
    'Ein HTA stellt nur einen String bereit, der WSH dagegen mehrere Dictionaries.
    'Diese Routine stellt zunächst für beide einen kompatiblen String bereit,
    'der als Grundlage zur Ermittlung der Parameter dient...
    dim i, oMatch, oMatches, ArgZeileTmp, idx, Trenner, Anf, Arg, ArgName, ArgString
    dim DateiAbsolut, oDateiliste, Dateiname
    dim Muster_BenanntOhneWert(1), Muster_BenanntMitWert(2), Muster_Unbenannt(1)
  
    ArgZeile = ""
        
    'Erkennungssmuster. ACHTUNG: Muster sind auf diese Reihenfolge abgestimmt => nicht ändern!
    Muster_BenanntMitWert(0)  = " """ & PrefixNamedArg & "([^\s"":]+):([^""]+)"""   'Mit Wertangabe, komplett in Anführungszeichen (Doppelpunkt nach Name ist Pflicht).
    Muster_BenanntMitWert(1)  =   " " & PrefixNamedArg & "([^\s"":]+):""([^""]+)""" 'Mit Wertangabe, Wert in Anführungszeichen (Doppelpunkt nach Name ist Pflicht).
    Muster_BenanntMitWert(2)  =   " " & PrefixNamedArg & "([^\s"":]+):([^ ""]+)"    'Mit Wertangabe, ohne Anführungszeichen (Doppelpunkt nach Name ist Pflicht).
    Muster_BenanntOhneWert(0) = " """ & PrefixNamedArg & "([^\s"":]+):?"""          'Ohne Wertangabe, mit Anführungszeichen (Doppelpunkt nach Name ist optional).
    Muster_BenanntOhneWert(1) =   " " & PrefixNamedArg & "([^\s"":]+):?"            'Ohne Wertangabe, ohne Anführungszeichen (Doppelpunkt nach Name ist optional).
    Muster_Unbenannt(0)       = " ""([^""]+)"""                                     'mit Anführungszeichen.
    Muster_Unbenannt(1)       = "([^ ""]+)"                                         'ohne Anführungszeichen.
    
    set oArgsNamed       = CreateObject("Scripting.Dictionary")
    set oArgsUnnamed     = CreateObject("Scripting.Dictionary")
    set oArgsDateinamen  = CreateObject("Scripting.Dictionary")
    set oArgsDateimasken = CreateObject("Scripting.Dictionary")
    set oArgsSonstige    = CreateObject("Scripting.Dictionary")
    set oDateiliste      = CreateObject("Scripting.Dictionary")
    
    '1. Unterschiede von WSH und IE egalisieren.
    if ((SkripthostName = "cscript.exe") or (SkripthostName = "wscript.exe")) then
      'Alle Argumente in einem gemeinsamen String bereitstellen (jedes Argument in Anführungszeichen).
      set oArgs = wscript.arguments
      For i = 0 to oArgs.Count-1
        ArgZeile = ArgZeile & " """ & oArgs.item(i) & """"
      next
      set oArgs = nothing
    else
      'Browser: Die HTA-Anwendung muß mit id="app" deklariert sein.
      ArgZeile = trim(app.commandLine)
      
      'Erstes Argument ist PfadName der HTA-Datei ==> als "SkriptPfadName" extrahieren.
      if (left(ArgZeile, 1) = """") then
        Trenner = """"
        Anf = 2
      else
        Trenner = " "
        Anf = 1
      end if
      idx = instr(2, ArgZeile, Trenner, vbTextCompare)
      if (idx < 1) then idx = len(ArgZeile) + Anf
      SkriptPfadName = mid(ArgZeile, Anf, idx - Anf)
      
      'SkriptPfadName aus Kommandozeile löschen, da inkompatibel mit "wscript.arguments.Unnamed".
      ArgZeile = mid(ArgZeile, idx + 1)
    end if
    ArgZeile = trim(ArgZeile)
    'ArgZeile = kompatible Kommandozeile für (w|c)script und IE/HTA.
        
    
    '2. Parametererkennung.
    ArgZeileTmp        = " " & ArgZeile & " "
    oRegExp.IgnoreCase = true
    oRegExp.Global     = true
    
    if (ArgZeile <> "") then
      'Benannte Parameter mit Wertangabe.
      for i = 0 to ubound(Muster_BenanntMitWert)
        oRegExp.Pattern = Muster_BenanntMitWert(i)
        set oMatches = oRegExp.Execute(ArgZeileTmp)
        for each oMatch in oMatches
          ArgName = lcase(oMatch.SubMatches(0))
          if (oArgsNamed.exists(ArgName)) then oArgsNamed.Remove(ArgName)
          oArgsNamed.add ArgName, oMatch.SubMatches(1)
        next
        ArgZeileTmp = oRegExp.Replace(ArgZeileTmp, "")
      next
      
      'Benannte Parameter ohne Wertangabe.
      for i = 0 to ubound(Muster_BenanntOhneWert)
        oRegExp.Pattern = Muster_BenanntOhneWert(i)
        set oMatches = oRegExp.Execute(ArgZeileTmp)
        for each oMatch in oMatches
          ArgName = lcase(oMatch.SubMatches(0))
          if (oArgsNamed.exists(ArgName)) then oArgsNamed.Remove(ArgName)
          oArgsNamed.add ArgName, ""
        next
        ArgZeileTmp = oRegExp.Replace(ArgZeileTmp, "")
      next
      
      'Unbenannte Argumente.
      for i = 0 to ubound(Muster_Unbenannt)
        oRegExp.Pattern = Muster_Unbenannt(i)
        set oMatches = oRegExp.Execute(ArgZeileTmp)
        for each oMatch in oMatches
          oArgsUnnamed.add oArgsUnnamed.count, oMatch.SubMatches(0)
        next
        ArgZeileTmp = oRegExp.Replace(ArgZeileTmp, "")
      next
      
      'Kategorisierung der unbenannten Parameter in:
      ' - Namen existierender Dateien.
      ' - Dateimasken mit Wildcards.
      '   ==> Für beide gilt: Falls kein absoluter Pfad enthalten ist,
      '       wird dieser mit Hilfe des Arbeitsverzeichnisses gebildet.
      ' - Restliche Parameter.
      oRegExp.Pattern = "[*\?]"
      for each idx in oArgsUnnamed
        Arg = oArgsUnnamed(idx)
        DateiAbsolut = fs.GetAbsolutePathName(Arg)
        'Dateimasken mit Wildcards erkennen => Jeder Parameter mit "?" oder "*" wird als ein solcher "erkannt" :-)
        if (oRegExp.test(DateiAbsolut)) then
          'Argument ist eine Dateimaske.
          oArgsDateimasken.add oArgsDateimasken.count, DateiAbsolut
          ArgZeileUnnamed3 = ArgZeileUnnamed3 & " """ & DateiAbsolut & """"
          
          'Dateimaske durch entsprechende existierende Dateeinamen ersetzen.
          oDateiliste.RemoveAll
          call FindeDateien_1Maske(DateiAbsolut, oDateiliste, false)
          for each Dateiname in oDateiliste
            ArgZeileUnnamed4 = ArgZeileUnnamed4 & " """ & Dateiname & """"
          next
          
        elseif (fs.FileExists(DateiAbsolut)) then
          oArgsDateinamen.add oArgsDateinamen.count, DateiAbsolut
          ArgZeileUnnamed3 = ArgZeileUnnamed3 & " """ & DateiAbsolut & """"
          ArgZeileUnnamed4 = ArgZeileUnnamed4 & " """ & DateiAbsolut & """"
        else
          oArgsSonstige.add oArgsSonstige.count, Arg
          ArgZeileUnnamed3 = ArgZeileUnnamed3 & " """ & Arg & """"
          ArgZeileUnnamed4 = ArgZeileUnnamed4 & " """ & Arg & """"
        end if
      next
  
    end if
    
    'Alle benannten Kommandozeilenparameter in einer Zeichenkette vereinen.
    for each ArgName in oArgsNamed
      ArgString = PrefixNamedArg & ArgName
      if (oArgsNamed(ArgName) <> "") then ArgString = ArgString & ":" & oArgsNamed(ArgName)
      ArgZeileNamed  = ArgZeileNamed  & " " & ArgString
      ArgZeileNamed2 = ArgZeileNamed2 & " """ & ArgString & """"
    next
    ArgZeileNamed  = trim(ArgZeileNamed)
    ArgZeileNamed2 = trim(ArgZeileNamed2)
    
    'Alle unbenannten Kommandozeilenparameter in einer Zeichenkette vereinen.
    for each idx in oArgsUnnamed
      ArgZeileUnnamed  = ArgZeileUnnamed  & " " & oArgsUnnamed(idx)
      ArgZeileUnnamed2 = ArgZeileUnnamed2 & " """ & oArgsUnnamed(idx) & """"
    next
    ArgZeileUnnamed  = trim(ArgZeileUnnamed)
    ArgZeileUnnamed2 = trim(ArgZeileUnnamed2)
    
    'Alle Dateimasken-Parameter in einer Zeichenkette vereinen.
    for each idx in oArgsDateimasken
      ArgZeileDateimasken  = ArgZeileDateimasken  & " " & oArgsDateimasken(idx)
      ArgZeileDateimasken2 = ArgZeileDateimasken2 & " """ & oArgsDateimasken(idx) & """"
    next
    ArgZeileDateimasken  = trim(ArgZeileDateimasken)
    ArgZeileDateimasken2 = trim(ArgZeileDateimasken2)
    
    'Alle Dateinamen-Parameter in einer Zeichenkette vereinen.
    for each idx in oArgsDateinamen
      ArgZeileDateinamen  = ArgZeileDateinamen  & " " & oArgsDateinamen(idx)
      ArgZeileDateinamen2 = ArgZeileDateinamen2 & " """ & oArgsDateinamen(idx) & """"
    next
    ArgZeileDateinamen  = trim(ArgZeileDateinamen)
    ArgZeileDateinamen2 = trim(ArgZeileDateinamen2)
    
    'Alle sonstigen unbenannten Kommandozeilenparameter in einer Zeichenkette vereinen.
    for each idx in oArgsSonstige
      ArgZeileSonstige  = ArgZeileSonstige  & " " & oArgsSonstige(idx)
      ArgZeileSonstige2 = ArgZeileSonstige2 & " """ & oArgsSonstige(idx) & """"
    next
    ArgZeileSonstige  = trim(ArgZeileSonstige)
    ArgZeileSonstige2 = trim(ArgZeileSonstige2)
    
    set oDateiliste = Nothing
  end sub
  
  
  
  public sub DeleteNamedArg(byVal ArgName)
    '"ArgName" löschen aus "oArgsNamed" und "ArgZeileNamed".
    dim ArgString
    if (oArgsNamed.exists(ArgName)) then
      oArgsNamed.Remove(ArgName)
      ArgZeileNamed  = ""
      ArgZeileNamed2 = ""
      for each ArgName in oArgsNamed
        ArgString = PrefixNamedArg & ArgName
        if (oArgsNamed(ArgName) <> "") then ArgString = ArgString & ":" & oArgsNamed(ArgName)
        ArgZeileNamed  = ArgZeileNamed  & " " & ArgString
        ArgZeileNamed2 = ArgZeileNamed2 & " """ & ArgString & """"
      next
      ArgZeileNamed  = trim(ArgZeileNamed)
      ArgZeileNamed2 = trim(ArgZeileNamed2)
    end if
  end sub
  
  
  private sub GetInternational() 
    'Ermittelt das in den Ländereinstellungen der Windows-Systemsteuerung festgelegte
    'Listentrennzeichen und Dezimaltrennzeichen. (bei Fehler ist der Rückgabewert = "?")
    '==> in Excel geht's auch so: Application.International(xlListSeparator)
    'On Error Resume Next
    Const Locale_RegValue   = "HKEY_CURRENT_USER\Control Panel\International\Locale"
    Const sList_RegValue    = "HKEY_CURRENT_USER\Control Panel\International\sList"
    Const sDecimal_RegValue = "HKEY_CURRENT_USER\Control Panel\International\sDecimal"
    
    If (RegValueExists(Locale_RegValue)) Then
      LangID = "&H" & WSHShell.RegRead(Locale_RegValue)
      On Error Resume Next
      LangID = LangID + 0
      if (Err.Number <> 0) then
        LangID = 0
        Err.clear
      end if
      On Error GoTo 0
    Else
      LangID = 0
    End If
    
    
    If (RegValueExists(sList_RegValue)) Then
      ListenTrenner = WSHShell.RegRead(sList_RegValue)
    Else
      ListenTrenner = "?"
    End If
    
    If (RegValueExists(sDecimal_RegValue)) Then
      DezimalTrenner = WSHShell.RegRead(sDecimal_RegValue)
    Else
      DezimalTrenner = "?"
    End If
    
    On Error GoTo 0
  End sub
  
  
  
  private Function RegValueExists(byVal value)
    ' prüft, ob RegistryValue "value" mit Standardeintrag existiert
    ' Rückgabewert: true oder false
    ' endet Value mit "\", so ist der Standardeintrag des angegebenen keys gemeint
    
    ' Lesen eines fehlenden Keys:
    '    err.number      = 0x80070002
    '    err.description = Ungültige Wurzel in Registrierungsschlüssel "HKCU\blah\".
    
    ' Lesen eines vorhandenen Keys ohne Standardwert:
    '    err.number      = 0x80070002 (siehe oben!)
    '    err.description = Registrierungsschlüssel "HKCU\*\" wurde nicht zum Lesen geöffnet
  
    dim Message_Text
    On Error Resume Next
    WSHShell.regread(ucase(value))
    if (err.number = 0) then 
      RegValueExists = true
    else 
      RegValueExists = false
    end if
    Message_Text = "value = " & value & vbNewLine & "Fehlernr = 0x" & hex(err.number) & vbNewline &_
                   "Fehlerbeschreibung = " & err.description & vbNewLine &_
                   "RegValueExists = " & RegValueExists
    debugecho Message_Text
    On Error Goto 0
    
  End Function
  
  
  
  private Function FindeDateien_1Maske(byVal strDateiMaske, byRef oPassendeDateien, byVal MitUnterverz)
    'Findet alle Dateien, die der angegebenen DateiMaske entsprechen.
    'Parameter: strDateiMaske    = [Pfad]Name.ext mit Wildcards,
    '                              falls leer oder ungültig, so ist diese Funktion wirkungslos.
    '           MitUnterverz     = Unterverzeichnisse durchsuchen (ja/nein)
    '           oPassendeDateien = Dictionary, in die gefundene Dateinamen incl. Pfad als Key
    '                              geschrieben werden; ist dieses Dictionary beim Funktions-
    '                              aufruf nicht leer, so werden die neu gefundenen Dateien 
    '                              dazugefügt. Keine Datei wird doppelt in die Liste aufgenommen.
    '                              ==> Key = Pfad\Name, Item = Name ohne Pfad.
    'Funktionswert: Anzahl gefundener Dateien
    
    Dim oDateiMaskeFiles, oDateiMaskeVerz, oDateiMaskeSubFolders, SubFolder
    Dim DateiMaskeName, DateiMaskePfad, DateiMaskeVerz, DateiMaskePfad_RegEx, Maske2
    Dim file, Anz, AnzA, k, PassendeDateienItems, FileVorhanden
    
    AnzA = oPassendeDateien.count 
    
    DateiMaskeName = fs.GetFileName(strDateiMaske)
    DateiMaskePfad = fs.GetAbsolutePathName(strDateiMaske)  ' wegen Bug (?) ist nur der Verzeichnisname i.O.!
    DateiMaskeVerz = fs.GetParentFolderName(DateiMaskePfad)
    if (right(DateiMaskeVerz, 1) = "\") then DateiMaskeVerz = left(DateiMaskeVerz, len(DateiMaskeVerz)-1)
    DateiMaskePfad = DateiMaskeVerz & "\" & DateiMaskeName
    
    DebugEcho "Dateisuche:"
    DebugEcho "DateiMaske übergeben:  '" & strDateiMaske & "'"
    DebugEcho "DateiMaskeName:        '" & DateiMaskeName & "'"
    DebugEcho "DateiMaskeVerz:        '" & DateiMaskeVerz & "'"
    DebugEcho "DateiMaskePfad:        '" & DateiMaskePfad & "'"
    
    if (not fs.FolderExists(DateiMaskeVerz)) then
      ErrEcho "Verzeichnis für Dateisuche existiert nicht: '" & DateiMaskeVerz & "'."
    else
      set oDateiMaskeVerz  = fs.GetFolder(DateiMaskeVerz)
      set oDateiMaskeFiles = oDateiMaskeVerz.Files
      
      DateiMaskePfad_RegEx = FileSpec2RegExp(DateiMaskePfad)
      DateiMaskePfad_RegEx = "^" & DateiMaskePfad_RegEx & "$"
      Debugecho "Suchmuster (RegExp): '" & DateiMaskePfad_RegEx & "'"
      for Each file in oDateiMaskeFiles
        if (entspricht(DateiMaskePfad_RegEx, file, IgnoreCase_Yes)) then
          'Ein Fehler tritt auf, wenn gefundene Datei bereits im Dictionary vorhanden ist.
          on error resume next
          oPassendeDateien.Add file, file.name
          on error goto 0
        end if
      next
      Anz = oPassendeDateien.count - AnzA
      Debugecho "==> " & Anz & " Dateien gefunden."
      'if (Anz > 0) then Debugecho ListeDictionary(oPassendeDateien)
      
      'Unterverzeichnisse durchsuchen.
      if (MitUnterverz) then
        Debugecho "Unterverzeichnisse von " & oDateiMaskeVerz.Path & ":"
        set oDateiMaskeSubFolders = oDateiMaskeVerz.SubFolders
        if (oDateiMaskeSubFolders.count > 0) then Debugecho ListeAuflistung(oDateiMaskeSubFolders)
        for each SubFolder in oDateiMaskeSubFolders
          Maske2 = SubFolder.path & "\" & DateiMaskeName
          call FindeDateien_1Maske(Maske2, oPassendeDateien, MitUnterverz)
        next
      end if
    end if
    
    set oDateiMaskeVerz  = nothing
    set oDateiMaskeFiles = nothing
    set oDateiMaskeSubFolders = nothing
    
    FindeDateien_1Maske = oPassendeDateien.count - AnzA
  End Function
  
  
  private Function FileSpec2RegExp(ByVal Spec)
    'Steve Fulton, geändert...
    'convert a filespec to a pattern used for Regular expressions.
    Dim Pattern
    With oRegExp
      .Global = True
      .Pattern = "\\"
      Pattern = .Replace(Spec, "\\")
      .Pattern = "[.(){}[\]$^]"
      Pattern = .Replace(Pattern, "\$&")
      .Pattern = "\?"
      Pattern = .Replace(Pattern, ".")
      .Pattern = "\*"
      Pattern = .Replace(Pattern, ".*")
    End With
    'FileSpec2RegExp = "^" & Pattern & "$"
    FileSpec2RegExp = Pattern
  End Function
  
  
  private Function entspricht(byVal Suchmuster, byVal Zeichenfolge, byVal blnIgnoreCase)
    'Test einer Zeichenfolge gegen einen regulären Ausdruck.
    'Parameter:     blnIgnoreCase   ...  Groß-/Kleinschreibung ignorieren?
    'Funktionswert: "true", wenn Zeichenfolge dem Suchmuster entspricht.
    On Error GoTo 0
    oRegExp.Pattern    = Suchmuster    ' Setzt das Muster.
    oRegExp.IgnoreCase = blnIgnoreCase ' Ignoriert die Schreibweise.
    oRegExp.Global     = False         ' Legt globales Anwenden fest.
    If oRegExp.test(Zeichenfolge) Then
      entspricht = True
    Else
      entspricht = False
    End If
  End Function
 
end class

' --- Tools_1.vbi.vbi ----------------------------------------------------
class Tools_1
  
  '...  Deklarationen  ****************************************************************************.
  
  public  UEditExe, CEditExe, AutoVueExe, BentleyViewExe, ViewCompExe, BrowserExe, ExcelExe
  public  JAVA_HOME, JavaExe, JEDIT_HOME, JEDIT_SETTINGS, jEditJar, Editoren, cKeinEditor
  private WshEnv, WshShell, fs, oRegExp, oFileErrors
  private RegValueEditor, strEditor
  
  
  
  '===  Variablen-Belegung  ========================================================================
  
  Private Sub Class_Initialize()
    
    dim i
    
    set WshShell    = CreateObject("WScript.Shell")
    set WshEnv      = WshShell.Environment("Process")
    set fs          = CreateObject("Scripting.FileSystemObject")
    set oFileErrors = CreateObject("Scripting.Dictionary")
    Set oRegExp     = New RegExp
    
    oSkript.debugecho "Klasse 'Tools_1' wird initialisiert."
    
    '"Konstanten"
    cKeinEditor    = "KeinEditor"
    RegValueEditor = "HKCU\Software\VB and VBA Program Settings\Common\Editor"    'Registry-Value für aktuell zu verwendenden Editor
    
    
    'Ermitteln diverser Pfade installierter Anwendungen.
    UEditExe       = GetUEditExe
    CEditExe       = GetCEditExe
    AutoVueExe     = GetAutoVueExe
    BentleyViewExe = GetBentleyViewExe
    ViewCompExe    = GetViewCompExe
    BrowserExe     = GetBrowserExe
    ExcelExe       = GetExcelExe
    JavaExe        = GetJavaExe
    call GetJEditUmgebung
    
    'Liste unterstützter Editoren aufbauen, Standardeditor setzen.
    call InitEditoren
    
    oSkript.debugecho "Exe UltraEdit = '" & UEditExe & "'."
    oSkript.debugecho "Exe Crimson Editor = '" & CEditExe & "'."
    oSkript.debugecho "Exe AutoVue = '" & AutoVueExe & "'."
    oSkript.debugecho "Exe BentleyView = '" & BentleyViewExe & "'."
    oSkript.debugecho "Exe ViewCompanion = '" & ViewCompExe & "'."
    oSkript.debugecho "Exe Browser = '" & BrowserExe & "'."
    oSkript.debugecho "Exe Excel = '" & ExcelExe & "'."
    oSkript.debugecho "Exe Java = '" & JavaExe & "'."
    oSkript.debugecho "Jar jEdit = '" & jEditJar & "'."
    oSkript.debugecho "Aktueller Editor = '" & strEditor & "'."
    oSkript.debugecho "Klasse 'Tools_1' erfolgreich instanziert."
  end sub
  
  
  Private Sub Class_Terminate()
    set WshEnv      = nothing
    set WshShell    = nothing
    set fs          = nothing
    set oFileErrors = nothing
    Set oRegExp     = nothing
    oSkript.debugecho "Klasse 'Tools_1' beendet."
  end sub
  
  
  '===  Eigenschaften  ============================================================================
  
  
  public property get Editor()
    Editor = strEditor
  end property
  
  public property let Editor(inpEditor)
    'Setzt den angegebenen Editor aktiv und speichert diesen in der Registry,
    'falls inpEditor eine gültige Kennung für einen verfügbaren Editor ist.
    dim i, Erfolg, vName, vInhalt, vTyp 
    Erfolg = false
    for i = 0 to ubound(Editoren, 1)
      if (Editoren(i, 1) = inpEditor) then
        strEditor = inpEditor
        'Editor merken
        vName   = RegValueEditor
        vInhalt = strEditor
        vTyp    = "REG_SZ"
        call RegistriereWert(vName, vInhalt, vTyp)
        oSkript.debugecho "Editor gesetzt auf '" & inpEditor & "'."
        Erfolg = true
        exit for
      end if
    next
    if (not Erfolg) then
      oSkript.debugecho "Editor konnte nicht gesetzt werden auf '" & inpEditor & "'."
    end if
  end property
  
  
  
  '===  Methoden  =================================================================================
  
  
  
  ' ***  Abteilung Reguläre Ausdrücke  ************************************************************
  
  Function entspricht(byVal Suchmuster, byVal Zeichenfolge, byVal blnIgnoreCase)
    'Test einer Zeichenfolge gegen einen regulären Ausdruck.
    'Parameter:     blnIgnoreCase   ...  Groß-/Kleinschreibung ignorieren?
    'Funktionswert: "true", wenn Zeichenfolge dem Suchmuster entspricht.
    On Error GoTo 0
    oRegExp.Pattern    = Suchmuster    ' Setzt das Muster.
    oRegExp.IgnoreCase = blnIgnoreCase ' Ignoriert die Schreibweise.
    oRegExp.Global     = False         ' Legt globales Anwenden fest.
    If oRegExp.test(Zeichenfolge) Then
      entspricht = True
    Else
      entspricht = False
    End If
  End Function
  
  
  
  Function substitute(ByVal Suchmuster, ByVal Ersatzstring, ByVal Zeichenfolge, _
                      ByVal blnAlleErsetzen, ByVal blnIgnoreCase)
    'Ersetzt das "Suchmuster" durch den "Ersatzstring" in der "Zeichenfolge".
    'Parameter: blnAlleErsetzen ... Alle Fundstellen ersetzen?
    'Parameter: blnIgnoreCase   ...  Groß-/Kleinschreibung ignorieren?
    'Funktionswert: Ergebniszeichenfolge (bei Mißerfolg = "Zeichenfolge")
    On Error GoTo 0
    oRegExp.Pattern    = Suchmuster        ' Setzt das Muster.
    oRegExp.IgnoreCase = blnIgnoreCase     ' Ignoriert die Schreibweise. (Namen in Excel sind nicht case sensitive!)
    oRegExp.Global     = blnAlleErsetzen   ' Legt globales Anwenden fest.
    If oRegExp.test(Zeichenfolge) Then
      substitute = oRegExp.Replace(Zeichenfolge, Ersatzstring)   ' Führt die Ersetzung durch.
    Else
      'ErrMessage = "Fehler beim Ersetzen:" & vbNewLine & vbNewLine & _
      '             "Suchmuster '" & Suchmuster & "' nicht gefunden."
      'MsgBox ErrMessage, vbExclamation, "Fehler"
      substitute = Zeichenfolge     ' keine Änderung
    End If
  End Function
  
  
  
  Function splitWords(byVal text, byRef Feld, byVal WordRegEx)
    'Splittet einen String auf Grundlage des für die Wortsuche (!) angegebenen
    'regulären Ausdruckes und gibt die gefundenen Wörter in einem Array zurück.
    'text      ... zu splittender String
    'Feld      ... Feld mit Ergebnissen (Wörtern), 1. Wort bei Index = 1!
    'WordRegEx ... reg. Ausdruck für die zu suchenden Wörter (nicht den Separator!)
    '              ==> wenn = "", dann wird der Standardausdruck verwendet.
    'Funktionswert = NF (Anzahl der gefundenen Felder)
    dim NF, cWords, i
    With oRegExp
      if (trim(WordRegEx) = "") then
        .Pattern = "\S+"
      else
        .Pattern = WordRegEx
      end if
      .Global     = True
      .IgnoreCase = false
      set cWords  = .Execute(text)
      NF = cWords.Count
    End With
    ReDim Feld(NF)
    Feld(0) = ""
    For i = 1 To NF
      Feld(i) = cWords(i - 1)
    Next
    splitWords = NF
    set cWords = Nothing
  end function
  
  
  Function FileSpec2RegExp(ByVal Spec)
    'Steve Fulton, geändert...
    'convert a filespec to a pattern used for Regular expressions.
    Dim Pattern
    With oRegExp
      .Global = True
      .Pattern = "\\"
      Pattern = .Replace(Spec, "\\")
      .Pattern = "[.(){}[\]$^]"
      Pattern = .Replace(Pattern, "\$&")
      .Pattern = "\?"
      Pattern = .Replace(Pattern, ".")
      .Pattern = "\*"
      Pattern = .Replace(Pattern, ".*")
      .Pattern = "\+"
      Pattern = .Replace(Pattern, "\+")
    End With
    'FileSpec2RegExp = "^" & Pattern & "$"
    FileSpec2RegExp = Pattern
  End Function
  
  
  ' ***  Abteilung Registry  **********************************************************************
  
  
  Function GetExeAusRegistry(byVal RegKey)
    'Funktionswert = Pfad\Dateiname der in RegKey registrierten und existierenden EXE oder "".
    'RegKey muß mit Backslash abgeschlossen sein, es sei denn, es ist ein RegValue!
    on error goto 0
    dim ExePfadName
    oSkript.debugecho "Pfad\Name einer Programmdatei ermitteln aus RegistryKey '" & RegKey & "'."
    if RegValueExists(RegKey) then
      ExePfadName = RegRead(RegKey)
      oSkript.debugecho "Inhalt des RegistryKey: '" & ExePfadName & "'"
      ExePfadName = trim(ExePfadName)
      'Platzhalter entfernen
      'ExePfadName = trim(substitute(" \""?%[0-9]\""?", "", ExePfadName, true, true))
      'Pfad\Name extrahieren:
      if (left(ExePfadName,1) = """") then
        'Pfad\Name ist in Anführungszeichen eingeschlossen.
        ExePfadName = MidStr(ExePfadName, 2, """", false)
        
      elseif (instr(1, ExePfadName, " ", vbTextCompare) > 0) then
        'Die gelesene Zeichenkette enthält Leerzeichen.
        if (not fs.FileExists(ExePfadName)) then
          'Der Pfad enthält keine Leerzeichen. Nach dem Leerzeichen stehen Parameter.
          '=> Ende bei erstem Leerzeichen.
          ExePfadName = LeftStr(ExePfadName, " ", false)
        else
          'Der Pfad enthält Leerzeichen => Die gesamte Zeichenkette ist der Pfad.
        end if
        
      else
        'Pfad\Name enthält weder Anführungszeichen noch Leerzeichen
        'alles schön ;-)
      end if
      'Anführungszeichen entfernen.
      'ExePfadName = trim(substitute("""", "", ExePfadName, true, true))
    end if
    oSkript.debugecho "Ermittelter Pfad\Name lautet: '" & ExePfadName & "'"
    if (fs.FileExists(ExePfadName)) then
      oSkript.debugecho "OK - '" & ExePfadName & "' existiert."
    else
      oSkript.debugecho "FEHLER - '" & ExePfadName & "' existiert nicht!"
      ExePfadName = ""
    end if
    GetExeAusRegistry = ExePfadName
  End Function
  
  
  Function RegKeyExists(byVal key)
    ' prüft, ob RegistryKey "key" existiert
    ' Rückgabewert: true oder false
    
    ' Lesen eines fehlenden Keys:
    '    err.number      = 0x80070002
    '    err.description = Ungültige Wurzel in Registrierungsschlüssel "HKCU\blah\".
    
    ' Lesen eines vorhandenen Keys ohne Standardwert:
    '    err.number      = 0x80070002 (siehe oben!)
    '    err.description = Registrierungsschlüssel "HKCU\*\" wurde nicht zum Lesen geöffnet
  
    dim Message_Text
    On Error Resume Next
    WshShell.regread(ucase(key))
    if (err.number = 0) then 
      RegKeyExists = true
    'elseif left(err.description, 43) = "Ungültige Wurzel in Registrierungsschlüssel" then
    elseif (left(err.description, 1) = "U") then
      RegKeyExists = false
    else 
      RegKeyExists = true
    end if
    Message_Text = "Key = " & key & vbNewLine & "Fehlernr = 0x" & hex(err.number) & vbNewline &_
                   "Fehlerbeschreibung = " & err.description & vbNewLine &_
                   vbNewLine & "RegKeyExists = " & RegKeyExists
    oSkript.debugecho Message_Text
    On Error Goto 0
    
  End Function
  
  
  
  Function RegValueExists(byVal value)
    ' prüft, ob RegistryValue "value" mit Standardeintrag existiert
    ' Rückgabewert: true oder false
    ' endet Value mit "\", so ist der Standardeintrag des angegebenen keys gemeint
    
    ' Lesen eines fehlenden Keys:
    '    err.number      = 0x80070002
    '    err.description = Ungültige Wurzel in Registrierungsschlüssel "HKCU\blah\".
    
    ' Lesen eines vorhandenen Keys ohne Standardwert:
    '    err.number      = 0x80070002 (siehe oben!)
    '    err.description = Registrierungsschlüssel "HKCU\*\" wurde nicht zum Lesen geöffnet
  
    dim Message_Text
    On Error Resume Next
    WSHShell.regread(ucase(value))
    if (err.number = 0) then 
      RegValueExists = true
    else 
      RegValueExists = false
    end if
    Message_Text = "value = " & value & vbNewLine & "Fehlernr = 0x" & hex(err.number) & vbNewline &_
                   "Fehlerbeschreibung = " & err.description & vbNewLine &_
                   "RegValueExists = " & RegValueExists
    oSkript.debugecho Message_Text
    On Error Goto 0
    
  End Function
  
  
  
  Function RegGetSubKeys(byVal Key, byRef oSubKeys)
    'Ermittelt alle direkten Unterschlüssel eines Registry-Keys (nur 1 Ebene)
    '(Export mit Reg.exe in TmpDatei, daraus Schlüssel lesen).
    
    'Parameter: Key        = RegistryKey, dessen SubKey-Namen ermittelt werden sollen
    '                      = -> keine Abkürzung des Stammschlüssels erlaubt, muß mit "\" enden!
    '           oSubKeys   = Dictionary, in dessen Key die gefundenen Unterschlüssel mit 
    '                        komplettem Pfad geschrieben werden; Als Item wird der Keyname 
    '                        selbst eingetragen. Ist dieses Dictionary beim Funktionsaufruf  
    '                        nicht leer, so werden die neu gefundenen Unterschlüssel dazugefügt.
    'Funktionswert         = true  ... Keys erfolgreich ermittelt
    '                        false ... Fehler beim exportieren des Key's, d.h. i.d.R. "Key ex. nicht"
    on error goto 0
    dim oRegDatei
    dim Zeile
    dim TmpDateiPfadName, Kommando, errCode, RegDateiAnzZeilen
    dim PosAnf, PosEnd, SubKey, i, Key1, SubKeySingleName
    
    Zeile = array()
    
    if RegKeyExists(Key) then
      TmpDateiPfadName = GetTmpDateiPfadName()
      Key  = lcase(Key)
      Key1 = """" & left(Key, len(Key)-1) & """"   ' Backslash entfernen, Anführungszeichen..
      'Kommando = "regedit /e " & TmpDateiPfadName & " " & Key1
      Kommando = "reg export " & Key1 & " " & TmpDateiPfadName
      errCode  = WshShell.Run(Kommando, WindowStyle_hidden, WaitOnReturn_yes)
      
      if ((errCode = 0) And fs.FileExists(TmpDateiPfadName)) then
        oSkript.debugecho "Lese temporäre Datei '" & TmpDateiPfadName & "'"
        Set oRegDatei = fs.OpenTextFile(TmpDateiPfadName, ForReading, false, TristateUseDefault)
        'RegDateiInhalt = oRegDatei.ReadAll
        i = 0
        Do While Not oRegDatei.AtEndOfStream
          i = i + 1
          redim preserve Zeile(i)
          Zeile(i) = lcase(oRegDatei.ReadLine)
          'WScript.Sleep 100
        Loop
        RegDateiAnzZeilen = i
        oRegDatei.close
        
        'oSkript.debugecho "Inhalt von " & TmpDateiPfadName & vbNewLine
        'For i = 1 To RegDateiAnzZeilen
        '  oSkript.debugecho "zeile " & i & "=" & Zeile(i)
        'Next
        
        oSkript.debugecho "gefundene Keys:" & vbNewLine
        For i = 1 To RegDateiAnzZeilen step 1
          PosAnf = Instr(Zeile(i), "[")
          PosEnd = Instr(Zeile(i), "]")
          if (PosAnf > 0 and PosEnd > 0) then
            SubKey = Mid(Zeile(i), PosAnf+1, PosEnd-PosAnf-1) & "\"
            if (SubKey <> Key) then
              SubKeySingleName = mid(Subkey,len(Key)+1,len(SubKey)-len(Key)-1)  ' Name ohne Stammkey
              if (instr(SubKeySingleName, "\") = 0) then
                ' nur SubKeys der 1. Ebene
                oSkript.debugecho "Zeile " & i & "=" & SubKey & " (" & SubKeySingleName & ")"
                oSubKeys.add SubKey, SubKeySingleName
              end if
            end if
          end if
        Next
        if not oSkript.DebugMode then fs.DeleteFile(TmpDateiPfadName)
        RegGetSubKeys = true
        Set oRegDatei = nothing
      else
        RegGetSubKeys = false
        oSkript.debugecho "Fehler bei Export:  '" & Key & "'."
      end if
    else
      RegGetSubKeys = false
      oSkript.debugecho "Key existiert nicht:'" & Key & "'."
    end if
    
  End Function
  
  
  Function RegZweigDelete(byVal Key)
    'Löscht einen RegistryKey mit allen Unterschlüsseln  (Export mit Regedit.exe 
    'in TmpDatei, daraus Schlüssel lesen, diese einzeln löschen mit WshShell.RegDelete)
    
    'Parameter: Key ... zu löschender RegistryKey
    'Funktionswert  ... true  = Key erfolgreich gelöscht
    '                   false = Fehler beim exportieren des Key's, d.h. i.d.R. "Key ex. nicht"
    on error goto 0
    dim oRegDatei
    dim Zeile
    dim TmpDateiPfadName, Kommando, errCode, RegDateiAnzZeilen
    dim Ausgabe, PosAnf, PosEnd, SubKey, i
    
    Zeile = array()
  
    TmpDateiPfadName = GetTmpDateiPfadName()
    if Right(Key,1)="\" then Key=left(Key, len(Key)-1)
    Key= """" & Key & """"
    Kommando = "regedit /e " & TmpDateiPfadName & " " & Key
    errCode  = WshShell.Run(Kommando, WindowStyle_hidden, WaitOnReturn_yes)
    'Alternative:
    'ExecMitPipe Kommando, Zeile, DebugMode    ' Kommando ausführen
    
    if fs.FileExists(TmpDateiPfadName) then
      oSkript.debugecho "Lese temporäre Datei '" & TmpDateiPfadName & "'"
      Set oRegDatei = fs.OpenTextFile(TmpDateiPfadName, ForReading, false, TristateUseDefault)
      'oRegDateiInhalt = oRegDatei.ReadAll
      i = 0
      Do While Not oRegDatei.AtEndOfStream
        i = i + 1
        redim preserve Zeile(i)
        Zeile(i) = oRegDatei.ReadLine
        'WScript.Sleep 100
      Loop
      RegDateiAnzZeilen = i
      oRegDatei.close
      
      'oSkript.debugecho "Inhalt von " & TmpDateiPfadName & vbNewLine
      'For i = 1 To RegDateiAnzZeilen
      '  oSkript.debugecho "zeile " & i & "=" & Zeile(i)
      'Next
      
      Ausgabe = "gefundene Keys:" & vbNewLine & vbNewLine
      For i = RegDateiAnzZeilen To 1 step -1
        PosAnf = Instr(Zeile(i), "[")
        PosEnd = Instr(Zeile(i), "]")
        if ((PosAnf > 0) and (PosEnd > 0)) then
          SubKey = Mid(Zeile(i), PosAnf+1, PosEnd-PosAnf-1) & "\"
          Ausgabe = Ausgabe & "zeile " & i & "=" & SubKey & vbNewLine
          on error goto 0      'Fehler löschen!
          on error resume next
          WshShell.RegDelete(SubKey)
          ' if err.number<>0 then msgbox "Fehler:" & vbNewline & "0x" & hex(err.number) & vbNewline & err.description
        end if
      Next
      oSkript.debugecho Ausgabe
      if not oSkript.DebugMode then fs.DeleteFile(TmpDateiPfadName)
      RegZweigDelete = true
      Set oRegDatei  = nothing
    else
      RegZweigDelete = false
      oSkript.debugecho "Key existiert nicht:'" & Key & "'."
    end if   
  
  End Function
  
  
  
  Sub RegZweigDelete_2(byVal sKey)
    'Alex K. Angelopoulos(MVP)
    'Removes Registry key and everything underneath it.
    Dim header, sData, FSO, sTmp, fTmp, tsTmp
    header = "REGEDIT4" & vbCrLf & vbCrLf
    sData = header & "[-" & sKey & "]" & vbCrLf
    sTmp = fs.GetAbsolutePathName(fs.GetTempName)
    fs.CreateTextFile(sTmp)
    set fTmp = fs.GetFile(sTmp)
    set tsTmp = fTmp.OpenAsTextStream(ForWriting, True)
    tsTmp.Write(sData): tsTmp.Close
    WshShell.Run "%COMSPEC% /C regedit /s " & Chr(34) & sTmp & Chr(34), 0, True
    fs.DeleteFile sTmp, True
  End Sub
  
  
  
  Function RegRead(ByVal KeyOrValue)
    'Rückgabewert: Wert des RegistryKey "KeyOrValue", im Zweifelsfalle leer
    'Endet "KeyOrValue" mit einem Backslash, so ist es ein Key, dessen Standardwert
    'gelesen wird.
    On Error Resume Next
    RegRead = WshShell.RegRead(UCase(KeyOrValue))
    On Error GoTo 0
  End Function
  
  
  
  Sub RegistriereWert (vName, vInhalt, vTyp)
    'Registriert beliebigen Wert in der Registry.
    'Parameter:  vName   ... vollständiger Wertname (ohne Backslash am Ende).
    '            vInhalt ... Text für den Eintraginhalt.
    '            vTyp    ... Datentyp des RegistryValue.
    oSkript.debugecho "Registriere Wert:" & vName & "=" & vInhalt & "(" & vTyp & ")"
    WSHShell.RegWrite vName, vInhalt, vTyp
  End Sub
  
  
  ' ***  Abteilung Dateien  ***********************************************************************
  
  Function GetTmpDateiPfadName()
    'Ermittelt kompletten Dateinamen für eine temporäre Datei.
    Dim otmpVerz, tmpDatName
    'Set otmpVerz        = fs.GetSpecialFolder(TempOrdner)
    tmpDatName          = fs.GetTempName
    'GetTmpDateiPfadName = otmpVerz.drive & "\" & otmpVerz.name & "\" & tmpDatName
    GetTmpDateiPfadName = fs.GetSpecialFolder(TempOrdner) & "\" & tmpDatName
  End Function
  
  
  Sub StartExcel(strArgumente)
    'Start von Excel mit den angegebenen Argumenten.
    dim Kommando
    if (ExcelExe = "") then
      oSkript.ErrEcho "StartExcel(): Excel ist nicht installiert."
    else
      Kommando = """" & ExcelExe & """ " & strArgumente
      oSkript.debugecho "Kommando wird ausgeführt: '" & Kommando & "'."
      On Error resume next
      call wshShell.Run(Kommando, WindowStyle_normal, WaitOnReturn_no)
      if (err = 0) then
        WshShell.AppActivate "Microsoft Excel"
      else
        oSkript.ErrEcho "StartExcel(): Excel konnte nicht gestartet werden."
      end if
      On Error GoTo 0
    end if
  end sub
  
  Function Dateiliste(Verzeichnis, Separator)
    'Funktionswert = String, der alle Dateinamen des angegebenen Verzeichnisses 
    'enthält - getrennt durch die Zeichenkette in "Separator"
    Dim Verz, Datei, Dateien, s
    set Verz    = fs.GetFolder(Verzeichnis)
    set Dateien = Verz.Files
    For Each Datei in Dateien
       s = s & Datei.name 
       s = s & Separator
    Next
    Dateiliste  = s
    set Verz    = nothing
    set Dateien = nothing
  End Function
  
  
  
  Function FindeDateien_1Maske(byVal strDateiMaske, byRef oPassendeDateien, byVal MitUnterverz)
  
    'Findet alle Dateien, die der angegebenen DateiMaske entsprechen.
    'Parameter: strDateiMaske    = [Pfad]Name.ext mit Wildcards,
    '                              falls leer oder ungültig, so ist diese Funktion wirkungslos.
    '           MitUnterverz     = Unterverzeichnisse durchsuchen (ja/nein)
    '           oPassendeDateien = Dictionary, in die gefundene Dateinamen incl. Pfad als Key
    '                              geschrieben werden; ist dieses Dictionary beim Funktions-
    '                              aufruf nicht leer, so werden die neu gefundenen Dateien 
    '                              dazugefügt. Keine Datei wird doppelt in die Liste aufgenommen.
    '                              ==> Key = Pfad\Name, Item = Name ohne Pfad.
    'Funktionswert: Anzahl gefundener Dateien
    
    Dim oDateiMaskeFiles, oDateiMaskeVerz, oDateiMaskeSubFolders, SubFolder
    Dim DateiMaskeName, DateiMaskePfad, DateiMaskeVerz, DateiMaskePfad_RegEx, Maske2
    Dim file, Anz, AnzA, k, PassendeDateienItems, FileVorhanden
    
    AnzA = oPassendeDateien.count 
    
    DateiMaskeName = fs.GetFileName(strDateiMaske)
    DateiMaskePfad = fs.GetAbsolutePathName(strDateiMaske)  ' wegen Bug (?) ist nur der Verzeichnisname i.O.!
    DateiMaskeVerz = fs.GetParentFolderName(DateiMaskePfad)
    if (right(DateiMaskeVerz, 1) = "\") then DateiMaskeVerz = left(DateiMaskeVerz, len(DateiMaskeVerz)-1)
    DateiMaskePfad = DateiMaskeVerz & "\" & DateiMaskeName
    
    oSkript.DebugEcho "Dateisuche:"
    oSkript.DebugEcho "DateiMaske übergeben:  '" & strDateiMaske & "'"
    oSkript.DebugEcho "DateiMaskeName:        '" & DateiMaskeName & "'"
    oSkript.DebugEcho "DateiMaskeVerz:        '" & DateiMaskeVerz & "'"
    oSkript.DebugEcho "DateiMaskePfad:        '" & DateiMaskePfad & "'"
    
    if (not fs.FolderExists(DateiMaskeVerz)) then
      oSkript.ErrEcho "Verzeichnis für Dateisuche existiert nicht: '" & DateiMaskeVerz & "'."
    else
      set oDateiMaskeVerz  = fs.GetFolder(DateiMaskeVerz)
      set oDateiMaskeFiles = oDateiMaskeVerz.Files
      
      DateiMaskePfad_RegEx = FileSpec2RegExp(DateiMaskePfad)
      DateiMaskePfad_RegEx = "^" & DateiMaskePfad_RegEx & "$"
      oSkript.Debugecho "Suchmuster (RegExp): '" & DateiMaskePfad_RegEx & "'"
      for Each file in oDateiMaskeFiles
        if (entspricht(DateiMaskePfad_RegEx, file, IgnoreCase_Yes)) then
          'Ein Fehler tritt auf, wenn gefundene Datei bereits im Dictionary vorhanden ist.
          on error resume next
          oPassendeDateien.Add file, file.name
          on error goto 0
        end if
      next
      Anz = oPassendeDateien.count - AnzA
      oSkript.Debugecho "==> " & Anz & " Dateien gefunden."
      if (Anz > 0) then oSkript.Debugecho ListeDictionary(oPassendeDateien)
      
      'Unterverzeichnisse durchsuchen.
      if (MitUnterverz) then
        oSkript.Debugecho "Unterverzeichnisse von " & oDateiMaskeVerz.Path & ":"
        set oDateiMaskeSubFolders = oDateiMaskeVerz.SubFolders
        if (oDateiMaskeSubFolders.count > 0) then oSkript.Debugecho ListeAuflistung(oDateiMaskeSubFolders)
        for each SubFolder in oDateiMaskeSubFolders
          Maske2 = SubFolder.path & "\" & DateiMaskeName
          call FindeDateien_1Maske(Maske2, oPassendeDateien, MitUnterverz)
        next
      end if
    end if
    
    set oDateiMaskeVerz  = nothing
    set oDateiMaskeFiles = nothing
    set oDateiMaskeSubFolders = nothing
  
    FindeDateien_1Maske = oPassendeDateien.count - AnzA
  End Function
  
  
  Function FindeDateien_xMasken(byVal oDateiMasken, byRef oPassendeDateien, _
                                byVal StandardMaske, byVal MitUnterverz)
  
    'Findet alle Dateien, die den angegebenen DateiMasken entsprechen.
    'Parameter: oDateiMasken     = mehrere Masken der Art: ([Pfad]Name.ext mit Wildcards )
    '                              Auflistung/Collection (oArgs) oder Dictionary mit 
    '                              Keys=0,1.. sowie Werten als Items!
    '           oPassendeDateien = Dictionary, in die gefundenen Dateinamen incl. Pfad 
    '                              geschrieben werden; ist dieses Dictionary beim Funktions-
    '                              aufruf nicht leer, so werden die neu gefundenen Dateien 
    '                              dazugefügt. Keine Datei wird doppelt in die Liste aufgenommen.
    '           StandardMaske    = String, wird als Dateimaske verwendet, falls oDateiMasken leer ist.
    '           MitUnterverz     = Unterverzeichnisse durchsuchen (ja/nein)
    'Funktionswert: Anzahl gefundener Dateien.
    'Verwendet die Funktion "FindeDateien_1Maske", um jeweils eine Dateimaske auszuwerten.
  
    Dim Maske, Anzahl, i
    Anzahl = 0
  
    if (oDateiMasken.count = 0) then
      'Keine Dateimaske angegeben: Standardmaske verwenden.
      Anzahl = Anzahl + FindeDateien_1Maske(StandardMaske, oPassendeDateien)
    else
      For i = 0 to oDateiMasken.Count-1
        Maske = oDateiMasken(i)
        'Durchläuft die Werte von Auflistung/Collection/Dictionary.
        Anzahl = Anzahl + FindeDateien_1Maske(Maske, oPassendeDateien, MitUnterverz)
      next
    end if
    FindeDateien_xMasken = Anzahl
  
  End Function
  
  
  
  Function LastBackslashDelete(ByVal Verzeichnis)
    'Funktionswert:           Verzeichnisname ohne Backslash am Ende
    'Parameter "Verzeichnis": Verzeichnisname mit oder ohne abschließenden Backslash
    Dim L
    L = Len(Verzeichnis)
    If (L > 0) Then
      If (Mid(Verzeichnis, L) = "\") Then Verzeichnis = Mid(Verzeichnis, 1, L - 1)
    End If
    LastBackslashDelete = Verzeichnis
  End Function
  
  
  
  Function NameExt(Pfad, MitOhneExt)
    'Gibt den Dateinamen ohne Pfad zurück. Ob die Extension mit zurückgegeben
    'wird, hängt vom zweiten Parameter ab.
    'Parameter "Pfad":       voll qualifizierter Dateiname
    'Parameter "MitOhneExt": wenn "mitext", dann enthält der Funktionswert
    '                        auch die in "Pfad" angegebene Extension, sonst nicht.
  
    dim lastBackslashAt, lastPointAt
    lastBackslashAt = InstrRev(Pfad, "\", -1, vbTextCompare)
    lastPointAt     = InstrRev(Pfad, ".", -1, vbTextCompare)
    'if lastBackslashAt=0 then lastBackslashAt=1-1
    if ((lastPointAt = 0) OR (MitOhneExt = "mitext")) then lastPointAt = len(Pfad) + 1
    NameExt = Mid(Pfad, lastBackslashAt + 1, lastPointAt - lastBackslashAt - 1)
  
  end function
  
  
  
  
  ' ***  Abteilung Zeichenketten  *****************************************************************
  
  Function LeftStr(sString, ByVal vDelimiter, ByVal bDelimiter)
    'Komfort-Ersatz für die Left-Funktion.
    '
    'vDelimiter: gibt entweder die Anzahl als Zahl an
    '            oder aber die gesuchte Zeichenkette, bis zu der
    '            der Teilstring zurückgegeben werden soll.
    'bDelimiter: true  ... vDelimiter ist Teil des Ergebnisses.
    '            false ... vDelimiter ist nicht Teil des Ergebnisses.
    Dim lPos
    If VarType(vDelimiter) = vbString Then
      lPos = InStr(sString, vDelimiter)
      If (lPos > 0) Then
        If (Not bDelimiter) Then
          lPos = lPos - 1
        else
          lPos = lPos - 1 + len(vDelimiter)
        end if
      end if
      If lPos < 1 Then lPos = 0
    Else
      lPos = cDbl(vDelimiter)
    End If
    LeftStr = Left(sString, lPos)
  End Function
  
  
  
  Function RightStr(sString, ByVal vDelimiter, ByVal bDelimiter)
    'Komfort-Ersatz für die Right-Funktion.
    '
    'vDelimiter: gibt entweder die Anzahl als Zahl an
    '            oder aber die gesuchte Zeichenkette, ab deren letztem
    '            Vorkommen der Teilstring zurückgegeben werden soll.
    '
    'bDelimiter: true  ... vDelimiter ist Teil des Ergebnisses.
    '            false ... vDelimiter ist nicht Teil des Ergebnisses.
    Dim lPos
    If VarType(vDelimiter) = vbString Then
      if (vDelimiter = "") then
        RightStr = ""
      else
        lPos = InStrRev(sString, vDelimiter)
        If lPos > 0 Then
          If (Not bDelimiter) Then
            lPos = lPos + len(vDelimiter)
          end if
          RightStr = Mid(sString, lPos)
        End If
      end if
    Else
      lPos = cDbl(vDelimiter)
      RightStr = right(sString, lPos)
    End If
  End Function
  
  
  
  Function MidStr(sString, ByVal vDelimiter1, ByVal vDelimiter2, ByVal bDelimiter)
    'Komfort-Ersatz für die Mid-Funktion.
    '
    'vDelimiter1: gibt entweder die Position als Zahl an
    '             oder aber das gesuchte Zeichen, ab der der Teilstring
    '             zurückgegeben werden soll.
    '
    'vDelimiter2: bestimmt die Länge des Teilstrings,
    '             der zurückgegeben werden soll.
    '             Wenn nicht leer, so wird der
    '             Teilstring ab vDelimiter1 und bis zum nächsten
    '             Vorkommen des in vDelimiter2 angegebenen Zeichens
    '             ermittelt.
    '
    'bDelimiter:  legt fest, ob der Teilstring einschl.
    '             dem der in vDelimiter1 und vDelimiter2 angegebenen
    '             Zeichen zurückgegeben werden soll (True) oder um
    '             je ein Zeichen links und rechts gekürzt (False).
    Dim lPos
    If (VarType(vDelimiter1) = vbString) Then
      lPos = InStr(sString, vDelimiter1)
      'If lPos > 0 And Not bDelimiter Then lPos = lPos + 1
      If ((lPos > 0) and (Not bDelimiter)) Then
        lPos = lPos + len(vDelimiter1)
      else
        lPos = lPos
      end if
      If lPos < 1 Then lPos = 0
    Else
      lPos = cDbl(vDelimiter1)
    End If
    
    If lPos > 0 Then
      If (VarType(vDelimiter2) = vbString) Then
        If vDelimiter2 = "" Then
          MidStr = Mid(sString, lPos)
        Else
          MidStr = LeftStr(Mid(sString, lPos), vDelimiter2, bDelimiter)
        end if
      else
        MidStr = LeftStr(Mid(sString, lPos), cDbl(vDelimiter2), bDelimiter)
      End If
    End If
  End Function
  
  
  
  Function GetZeileAusString(byRef Text)
    'Gibt die erste Zeile aus "Text" zurück und löscht diese im "Text"!
    dim Zeile, msg
  
    msg = "Text = '" & Text & "'."
    Zeile = leftStr(Text, vbNewLine, true)
  
    if (Zeile = "") then
      'letzte Zeile.
      Zeile = Text
      Text = ""
    else
      'Gefundene Zeile löschen.
      Text = MidStr(Text, Zeile, "", false)
    end if
	   Zeile = Replace(Zeile, vbNewLine, "")
  
    msg = "Zeile = '" & Zeile & "', Länge = '" & len(Zeile) & vbnewline & vbnewline & msg
    msg = msg & vbnewline & vbnewline & "Text = '" & Text & "'"
    'msgbox msg
  
    GetZeileAusString = Zeile
  end function
  
  
  
  Function Konv_OEM2ANSI(byVal text)
	   'Konversion der deutschen Umlaute
	   text = Replace(text, chr(148), chr(246))	'ö
	   text = Replace(text, chr(132), chr(228))	'ä
	   text = Replace(text, chr(129), chr(252))	'ü
	   text = Replace(text, chr(153), chr(214))	'Ö
	   text = Replace(text, chr(142), chr(196))	'Ä
	   text = Replace(text, chr(154), chr(220))	'Ü
	   text = Replace(text, chr(225), chr(223))	'ß
    Konv_OEM2ANSI = text
  end function
  
  
  
  Function NextChar(ByRef Buffer) 
    'returns the first character from a buffer and removes
    'it from the buffer
    NextChar = Mid(Buffer, 1, 1)
    Buffer = Mid(Buffer, 2)
  End Function
  
  
  ' ***  Abteilung Arrays, Sortieren  *************************************************************
  
  Function AnzDim(Feld)
    'Parameter: Feld ... ein- oder mehrdimensionales Array
    'Rückgabe:  Anzahl der Dimensionen von Feld, kann auch "0" sein.
    On Error Resume Next
    Dim idx
    Dim i
    i = 0
    Do
      i = i + 1
      idx = UBound(Feld, i)
    Loop Until (Err <> 0)
    On Error GoTo 0
    AnzDim = i - 1
  End Function
  
  
  
  sub TransposeArray2d(byRef Matrix)
    'Rückgabe: Transponiertes zweidimensionales Array "Matrix".
    'Eingabe:  Matrix        ... zu transponierendes Array
    dim TM
    dim MaxZeilen, MaxSpalten, ze, sp
    if (AnzDim(Matrix) = 2) then
      MaxZeilen  = UBound(Matrix, 1)
      MaxSpalten = UBound(Matrix, 2)
      redim TM(MaxSpalten, MaxZeilen)
      for ze = 0 to MaxZeilen
        for sp = 0 to MaxSpalten
          TM(sp, ze) = Matrix(ze, sp)
        next
      next
    end if
    Matrix = TM
  end sub
  
  ' ***  Abteilung Debug  *************************************************************************
  
  Function ListeDictionary(oDictionary)
    'Rückgabe: String mit Auflistung aller Einträge eines Dictionary zwecks Anzeige für Debug-Zwecke.
    '=> Das Dictionary muss dafür sehr einfach aufgebaut sein: Jeder Item ist ein String!
    Dim DictionaryItems, DictionaryKeys, Liste, i
    Dim StringCount
    Dim StringArray()
    
    DictionaryItems = oDictionary.Items
    DictionaryKeys  = oDictionary.Keys
    
    StringCount = 3 + oDictionary.Count
    ReDim StringArray(StringCount - 1)
    
    StringArray(0) = "Das Dictionary hat " & CStr(oDictionary.Count) & " Einträge"
    StringArray(1) = "Inhalt des Dictonary: " & vbNewLine & "i" & vbTab & "Key" & vbTab & "Eintrag" & vbNewLine & "-------------------------------------------------------------------"
    
    for i = 0 to UBound(DictionaryKeys)
      'Hinweise zur Performance:
      '1. Klammern der kurzen Strings bedeutet: Schleife braucht nur noch 17% der Zeit ohne Klammern
      '2. Ersetzen der Stringverkettung durch Speichern der einzelnen Zeilen im Array und Join() reduziert die Laufzeit auf 1%!!! 
      'Liste = Liste & (CStr(i) & vbTab & DictionaryKeys(i) & vbTab & DictionaryItems(i) & vbNewLine)
      
      StringArray(2 + i) = (CStr(i) & vbTab & DictionaryKeys(i) & vbTab & DictionaryItems(i))
    next
    StringArray(StringCount - 1) = vbNewLine
    
    ListeDictionary = Join(StringArray, vbNewLine)
  End Function
  
  
  Function ListeAuflistung(oAuflistung)
    'Rückgabe: String mit Auflistung aller Einträge einer Auflistung/Collection zwecks Anzeige für Debug-Zwecke.
    Dim Liste, i, Eintrag
    Liste = "Inhalt der Auflistung/Collection: " & vbNewLine & "i" & vbTab & "Eintrag" & vbNewLine & "-------------------------------------------------------------------" & vbNewLine
    i = 0
    for each Eintrag in oAuflistung
      i = i + 1
      Liste = Liste & (i & vbTab & Eintrag & vbNewLine)
    next
    Liste = Liste & vbNewLine
    ListeAuflistung = Liste
  End Function
  
  
  
  '===  interne Funktionen  =======================================================================
  
  Private Function GetUEditExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE von UEdit oder "".
    dim Key_UEditExe, UEditExe
    on error goto 0
    oSkript.debugecho "Programmdatei von UltraEdit ermitteln."
    
    'Versionen 6 bis 10:
    Key_UEditExe = "HKEY_CLASSES_ROOT\Applications\UEdit32.exe\shell\edit\command\"
    UEditExe = GetExeAusRegistry(Key_UEditExe)
    if (UEditExe = "") then
      Key_UEditExe = "HKEY_CLASSES_ROOT\UltraEdit-32 Document\shell\open\command\"
      UEditExe = GetExeAusRegistry(Key_UEditExe)
    end if
    
    'Version 11
    if (UEditExe = "") then
      'Voraussetzung für die Existenz des folgenden Schlüssels ist leider
      'die von UltraEdit aus erfolgte Dateizuordnung des Typs ".txt"!!!
      Key_UEditExe = "HKEY_CLASSES_ROOT\UltraEdit.txt\shell\open\command\"
      UEditExe = GetExeAusRegistry(Key_UEditExe)
    end if
    GetUEditExe = UEditExe
  End Function
  
  Private Function GetCEditExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE von CEdit oder "".
    dim Key_CEditExe, CEditExe, CEditVerz
    on error goto 0
    dim Erfolg
    const ExeName = "cedt.exe"
    Erfolg = false
    oSkript.debugecho "Programmdatei von Crimson Editor ermitteln."
    
    'Von hinten durchs Auge: Was deinstalliert werden kann, ist ja vielleicht vorhanden...
    Key_CEditExe = "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Crimson Editor 3.72\DisplayIcon"
    CEditExe = GetExeAusRegistry(Key_CEditExe)
    if (fs.FileExists(CEditExe)) then
      Erfolg = true
    else
      oSkript.debugecho "GetCEditExe(): Crimson Editor nicht gefunden in Registry => suche nach Portable Version"
    end if
    
    'Portable Version an 3 Stellen suchen
    if (not Erfolg) then
      CEditVerz = "G:\Tools\Crimson Editor"
      CEditExe  = CEditVerz & "\" & ExeName
      if (fs.FileExists(CEditExe)) then
        Erfolg = true
      else
        oSkript.debugecho "GetCEditExe(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditVerz & "'"
      end if
    end if
    
    if (not Erfolg) then
      CEditVerz = oSkript.OSDir_Programme & "\Crimson Editor"
      CEditExe  = CEditVerz & "\" & ExeName
      if (fs.FileExists(CEditExe)) then
        Erfolg = true
      else
        oSkript.debugecho "GetCEditExe(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditVerz & "'"
      end if
    end if
    
    if (not Erfolg) then
      CEditVerz = oSkript.OSDir_Programme & "\Tools\Crimson Editor"
      CEditExe  = CEditVerz & "\" & ExeName
      if (fs.FileExists(CEditExe)) then
        Erfolg = true
      else
        oSkript.debugecho "GetCEditExe(): cedt.exe nicht gefunden im (hart kodierten) Verzeichnis '" & CEditVerz & "'"
      end if
    end if
    
    if (not Erfolg) then CEditExe = ""
    
    GetCEditExe = CEditExe
  End Function
  
  
  Private Function GetExcelExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE von Excel oder "".
    dim Key_ExcelExe, ExcelExe, DatAlias_XLS, RegKeyDatAlias_XLS
    on error goto 0
    const RegKeyDatExt_XLS = "HKEY_CLASSES_ROOT\.xls\"
    oSkript.debugecho "Programmdatei von Excel ermitteln."
    if RegValueExists(RegKeyDatExt_XLS) then
      DatAlias_XLS       = WSHShell.RegRead(RegKeyDatExt_XLS)             'Datei-Alias für XLS's
      RegKeyDatAlias_XLS = "HKEY_CLASSES_ROOT\" & DatAlias_XLS & "\"      'Registry-Schlüssel mit XLS-Kontextmenü
      oSkript.debugecho "RegKeyDatAlias_XLS: '" & RegKeyDatAlias_XLS & "'"
      Key_ExcelExe = RegKeyDatAlias_XLS & "shell\open\command\"
      ExcelExe = GetExeAusRegistry(Key_ExcelExe)
    end if
    if (ExcelExe = "") then
      Key_ExcelExe = "HKEY_CLASSES_ROOT\Applications\Excel.exe\shell\open\command\"
      ExcelExe = GetExeAusRegistry(Key_ExcelExe)
    end if
    GetExcelExe = ExcelExe
  End Function
  
  
  Private Function GetAutoVueExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE von AutoVue oder "".
    on error goto 0
    dim Key_AutoVueExe
    Key_AutoVueExe = "HKEY_CLASSES_ROOT\AutoVue.Document\shell\open\command\"
    oSkript.debugecho "Programmdatei von AutoVue ermitteln."
    GetAutoVueExe = GetExeAusRegistry(Key_AutoVueExe)
  End Function
  
  
  Private Function GetBentleyViewExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE von BentleyView oder "".
    
    '**** funktioniert kaum  ***
    
    on error goto 0
    dim Key_BentleyViewExe
    Key_BentleyViewExe = "HKEY_CLASSES_ROOT\Applications\BentleyView.exe\shell\open\command\"
    oSkript.debugecho "Programmdatei von BentleyView ermitteln."
    GetBentleyViewExe = GetExeAusRegistry(Key_BentleyViewExe)
  End Function
  
  
  Private Function GetViewCompExe()
    'Funktionswert = Pfad\Dateiname der existierenden EXE von ViewCompanion Pro 4 oder "".
    'Gesucht wird in: - "G:\Tools\ViewCompanion"
    'Es wird auf eine plausible Seriennummer getestet.
    on error goto 0
    dim Erfolg, ViewComp_HOME, ViewCompanionExe, SerienNummer
    const ExeName            = "viewcompanion.exe"
    const Key_ViewCompLizenz = "HKEY_CURRENT_USER\Software\Software Companions\ViewCompanion Pro\Registration Pro\Serial Number"
    const Muster_Lizenz      = "\w+-\w+-\w+-\w+"
    
    Erfolg = false
    oSkript.debugecho "Pfad\Dateiname von " & ExeName & " ermitteln."
    
    'if (not Erfolg) then
      ViewComp_HOME = "G:\Tools\ViewCompanion"
      ViewCompanionExe = ViewComp_HOME & "\" & ExeName
      if (fs.FileExists(ViewCompanionExe)) then
        if (RegValueExists(Key_ViewCompLizenz)) then
          SerienNummer = RegRead(Key_ViewCompLizenz)
          if (entspricht(Muster_Lizenz, SerienNummer, IgnoreCase_Yes)) then
            'Lizenz scheint i.O => Start von ViewCompanion könnte gelingen.
            Erfolg = true
          end if
        end if
        if (not Erfolg) then
          oSkript.debugecho "GetViewCompExe(): SerienNummer '" & SerienNummer & "' entspricht nicht dem erwarteten Schema"
        else
          oSkript.debugecho "GetViewCompExe(): SerienNummer '" & SerienNummer & "' scheint i.O."
        end if
      else
        oSkript.debugecho "GetViewCompExe(): " & ExeName & " nicht gefunden im (hart kodierten) Verzeichnis '" & ViewComp_HOME & "'"
      end if
    'end if
    
    if (Erfolg) then
      oSkript.debugecho "Ermittelter Pfad\Name lautet: '" & ViewCompanionExe & "'"
    else
      ViewCompanionExe = ""
    end if
    
    GetViewCompExe = ViewCompanionExe
  End Function
  
  
  Private Function GetBrowserExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden EXE des Standard-Browser oder "".
    on error goto 0
    dim Key_BrowserExe
    Key_BrowserExe = "HKEY_CLASSES_ROOT\htmlfile\shell\open\command\"
    oSkript.debugecho "Programmdatei des Standard-Browser ermitteln."
    GetBrowserExe = GetExeAusRegistry(Key_BrowserExe)
  End Function
  
  
  Private Function GetJavaExe()
    'Funktionswert = Pfad\Dateiname der registrierten und existierenden Javaw.exe oder "".
    'legt folgende öffentlichen Variablen fest:
    'JAVA_HOME = Home-Verzeichnis von Java
    on error goto 0
    dim Key_JavaExe, Pfad_JavaExe, Erfolg
    const ExeName = "javaw.exe"
    Erfolg = false
    Key_JavaExe = "HKEY_CLASSES_ROOT\jarfile\shell\open\command\"
    oSkript.debugecho "Programmdatei von Java ermitteln."
    
    JAVA_HOME = LastBackslashDelete(WshEnv("JAVA_HOME"))
    oSkript.debugecho "GetJavaExe(): Umgebungsvariable %JAVA_HOME%='" & JAVA_HOME & "'"
    if (JAVA_HOME <> "") then
      Pfad_JavaExe = JAVA_HOME & "\bin\" & ExeName
      if (fs.FileExists(Pfad_JavaExe)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJavaExe(): javaw.exe nicht gefunden im Verzeichnis der Umgebungsvariable %JAVA_HOME%='" & JAVA_HOME & "'"
      end if
    end if
    
    if (not Erfolg) then
      Pfad_JavaExe = oSkript.OS_WindowsSys & "\" & ExeName
      if (fs.FileExists(Pfad_JavaExe)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJavaExe(): javaw.exe nicht gefunden im Windows Systemverzeichnis '" & oSkript.OS_WindowsSys & "'"
      end if
    end if
    
    if (not Erfolg) then Pfad_JavaExe = GetExeAusRegistry(Key_JavaExe)
    
    GetJavaExe = Pfad_JavaExe
  End Function
  
  
  Private sub GetJEditUmgebung()
    'legt folgende öffentlichen Variablen fest:
    'jEditjar   = Pfad\Dateiname der gefundenen Datei jEdit.jar oder "".
    'JEDIT_HOME = Verzeichnis von jEditjar
    on error goto 0
    dim Erfolg
    const JarName = "jEdit.jar"
    Erfolg = false
    oSkript.debugecho "Pfad\Dateiname von " & JarName & " ermitteln."
    
    JEDIT_HOME = LastBackslashDelete(WshEnv("JEDIT_HOME"))
    if (JEDIT_HOME <> "") then
      jEditjar = JEDIT_HOME & "\" & JarName
      if (fs.FileExists(jEditjar)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJEditUmgebung(): jEdit.jar nicht gefunden im Verzeichnis der Umgebungsvariable %JEDIT_HOME%='" & JEDIT_HOME & "'"
      end if
    end if
    
    if (not Erfolg) then
      JEDIT_HOME = "G:\Tools\jEdit"
      jEditjar = JEDIT_HOME & "\" & JarName
      if (fs.FileExists(jEditjar)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJEditUmgebung(): jEdit.jar nicht gefunden im (hart kodierten) Verzeichnis '" & JEDIT_HOME & "'"
      end if
    end if
    
    if (not Erfolg) then
      JEDIT_HOME = oSkript.OSDir_Programme & "\jEdit"
      jEditjar = JEDIT_HOME & "\" & JarName
      if (fs.FileExists(jEditjar)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJEditUmgebung(): jEdit.jar nicht gefunden im (hart kodierten) Verzeichnis '" & JEDIT_HOME & "'"
      end if
    end if
    
    if (not Erfolg) then
      JEDIT_HOME = oSkript.OSDir_Programme & "\Tools\jEdit"
      jEditjar = JEDIT_HOME & "\" & JarName
      if (fs.FileExists(jEditjar)) then
        Erfolg = true
      else
        oSkript.debugecho "GetJEditUmgebung(): jEdit.jar nicht gefunden im (hart kodierten) Verzeichnis '" & JEDIT_HOME & "'"
      end if
    end if
    
    JEDIT_SETTINGS = LastBackslashDelete(WshEnv("JEDIT_SETTINGS"))
    oSkript.debugecho "GetJEditUmgebung(): Umgebungsvariable %JEDIT_SETTINGS%='" & JEDIT_SETTINGS & "'"
    
    if (not Erfolg) then
      JEDIT_HOME = ""
      jEditjar   = ""
    end if
  End sub
  
  
  Private sub InitEditoren()
    'Füllt die Liste unterstützter Editoren und setzt den Standard-Editor.
    dim i, letzterEditor
    
    'Liste unterstützter und verfügbarer Editoren aufbauen.
    i = -1
    redim Editoren(1,i)
    if (UEditExe <> "") then
      i = i + 1
      redim preserve Editoren(1,i)
      Editoren(0,i) = "UltraEdit"   'sichtbarer Text in Listbox
      Editoren(1,i) = "UltraEdit"   'Wert der Listbox => muss in diesem Fall identisch sein mit dem Text.
    end if
    if (CEditExe <> "") then
      i = i + 1
      redim preserve Editoren(1,i)
      Editoren(0,i) = "Crimson Editor"   'sichtbarer Text in Listbox
      Editoren(1,i) = "Crimson Editor"   'Wert der Listbox => muss in diesem Fall identisch sein mit dem Text.
    end if
    if ((JEditJar <> "") and (JavaExe <> "")) then
      i = i + 1
      redim preserve Editoren(1,i)
      Editoren(0,i) = "jEdit"   'sichtbarer Text in Listbox
      Editoren(1,i) = "jEdit"   'Wert der Listbox => muss in diesem Fall identisch sein mit dem Text.
    end if
    'Keinen Editor gefunden.
    if (i = -1) then
      i = 0
      redim Editoren(1,i)
      Editoren(0,i) = "kein Editor verfügbar"
      Editoren(1,i) = cKeinEditor
    end if
    call TransposeArray2d(Editoren)
    
    
    'Standard-Editor setzen.
    strEditor = cKeinEditor
    'Zuletzt eingestellten Editor aus Registry lesen
    on error resume next
    letzterEditor = WSHShell.RegRead(RegValueEditor)
    on error goto 0
    Editor = letzterEditor
    if (strEditor = cKeinEditor) then Editor = Editoren(0,1)
    
  End sub
  
end class 

' --- XlTools.vbi --------------------------------------------------------
class XlTools
  
  '...  Deklarationen  ****************************************************************************.
  
  public  xlApp, ExcelNeuGestartet
  private WshShell
  
  
  '===  Variablen-Belegung  ========================================================================
  
  Private Sub Class_Initialize()
    set WshShell = CreateObject("WScript.Shell")
    ExcelNeuGestartet = false
    Set xlApp = nothing
    
    oSkript.debugecho "Klasse 'XlTools' instanziert."
  end sub
  
  Private Sub Class_Terminate()
    set WshShell = nothing
    set xlApp    = nothing
    oSkript.debugecho "Klasse 'XlTools' beendet."
  end sub
  
  
 ' Allgemeine Methoden  ---------------------------------------------------------------------------
  
  function GetExcelApp(Sichtbarkeit)
    'Öffnet Excel, wenn es noch nicht läuft.
    'Gibt das Excel.Application-Objekt zurück oder "nothing" bei Mißerfolg.
    'Parameter: Sichtbarkeit ... (true | false) Soll das Excel-Anwendungsfenster sichtbar werden?
    
    dim XlArbVerz
    
    'Hinweise zum Verhalten:
    'Wird Excel via CreateObject oder GetObject als Automatisierungsserver in Anspruch
    'genommen, dann aber manuell "geschlossen", so wird die Anwendung zwar unsichtbar, 
    'der Prozess "Excel.exe" aber bleibt erhalten.
    'Der Prozess verschwindet erst, wenn der Automatisierungsclient geschlossen bzw.
    'die Objektvariable auf "nothing" gesetzt wird.
    'Wird die Verbindung zu diesem unsichtbaren Prozess hier weiter in Anspruch genommen,
    'passieren allerdings unverständliche Dinge (evtl. nur mit GeoTools-AddIn ?)
    '=> Deshalb erst die Verbindung kappen und
    'neu aufbauen, auch wenn dabei Excel beendet und neu gestartet wird...
    
    Set xlApp = nothing
    on error resume next
    
    'Versuch, eine vorhandene Instanz von Excel zu finden
    set xlApp = GetObject(,"Excel.Application")
    if (err.number <> 0) then
      oSkript.echo "Keine vorhandene Instanz von Excel gefunden => Excel starten."
      on error resume next
      Set xlApp = CreateObject("Excel.Application")
      'Fehlstart abfangen.
      if (err.number = 0) then 
        on error goto 0
        oSkript.echo("Excel erfolgreich gestartet.")
        call LadeStartupAddins(xlApp)
        'Neue Mappe anlegen, damit Excel bei Funktionsende nicht beendet wird (?!).
        'xlApp.Workbooks.add
        ExcelNeuGestartet = true
      else
        oSkript.ErrEcho "FEHLER beim Start von Excel."
        ExcelNeuGestartet = false
      end if
    else
      'Vorhandene Instanz von Excel gefunden.
      'Diese blinkt ab sofort dümmlich in der Taskleiste vor sich hin.
      oSkript.echo "Vorhandene Instanz von Excel gefunden."
      'Excel in den VORDERGRUND bringen!
      WSHShell.AppActivate "Excel"      'funktioniert nur bedingt ... bis gar nicht.
      ExcelNeuGestartet = false
    end if
    on error goto 0
    
    if (not xlApp is nothing) then
      on error resume next
      oSkript.debugecho "Arbeitsverzeichnis in Excel setzen auf: '" & oSkript.ArbeitsVerz & "'."
      XlArbVerz = xlApp.run("SetArbeitsverzeichnis", oSkript.ArbeitsVerz)
      if (err.number = 0) then 
        oSkript.debugecho "Arbeitsverzeichnis in Excel gesetzt auf: '" & XlArbVerz & "'."
      else
        oSkript.ErrEcho "FEHLER beim Setzen des Arbeitsverzeichnisses."
      end if
      on error goto 0
      'Fensterstatus wird gesetzt, aber deshalb kommt das Fenster noch lange nicht in den Vordergrund.
      'xlApp.WindowState = xlMaximized
      xlApp.DisplayStatusBar = True
      SetExcelAppSichtbarkeit(Sichtbarkeit)
    end if
    
    set GetExcelApp = xlApp
  End function
  
  function SetExcelAppSichtbarkeit(Sichtbarkeit)
    'Steuert die Sichtbarkeit des Excel-Anwendungsfensters
    'Parameter: Sichtbarkeit ... (true | false) Soll das Excel-Anwendungsfenster sichtbar werden?
    'Rückgabe:  True bei Erfolg, sonst false
    '-----------------------------------------------------------------------------------------
    dim Erfolg
    oSkript.debugecho "XLTools\SetExcelAppSichtbarkeit(" & Sichtbarkeit & ")"
    Erfolg = true
    on error resume next
    if (not xlApp is nothing) then
      
      if (Sichtbarkeit) then
        if (Not xlApp.Visible) then xlApp.Visible = true
      else
        if (xlApp.Visible) then xlApp.Visible = false
      end If
      
      xlApp.ScreenUpdating = Sichtbarkeit
      xlApp.UserControl = Sichtbarkeit
    end if
    if (err.number <> 0) then
      Erfolg = false
      oSkript.ErrEcho "Fehler: Setzen der Excel-Sichtbarkeit auf '" & Sichtbarkeit & "' fehlgeschlagen!"
    end if
    on error goto 0
    SetExcelAppSichtbarkeit = Erfolg
  end function
  
  sub EntferneCSVTextkenner(byVal oRange)
    '-----------------------------------------------------------------------------------------
    'Entfernt führende Hochkommata in allen Spalten des angegebenen Bereiches,
    'die ausnahmslos als Text formatiert sind.
    '
    'Eingabe: oRange ... zu bearbeitender Bereich
    '                    Wenn = nothing, dann gilt: die aktive Auswahl, oder
    '                    falls diese nicht existiert, die gesamte Tabelle.
    '-----------------------------------------------------------------------------------------
    dim oZellen, Zelle, ZellInhalt
    dim AnzZeilen, AnzSpalten, Sp, oSpalte
    oSkript.debugecho "XLTools\EntferneCSVTextkenner()"
    'on error resume next
    if (not xlApp is nothing) then
      If (Not (xlApp.ActiveCell Is Nothing)) Then
        
        'Bearbeitungsbereich bestimmen
        if (not oRange is nothing) then
          set oZellen = oRange
        elseif (xlApp.Selection.Cells.Count > 1) then
          set oZellen = xlApp.Selection
        else
          set oZellen = xlApp.ActiveWorkbook.ActiveSheet.UsedRange
        end if
        oSkript.debugecho "XLTools\EntferneFuehrendeHochkommata(): Bereich = " & oZellen.Address
        
        AnzZeilen  = oZellen.Rows.Count
        AnzSpalten = oZellen.Columns.Count
        
        'Jede Spalte einzeln bearbeiten
        for Sp = 1 to oZellen.Columns.Count
          set oSpalte = oZellen.Range(xlApp.Cells(1, Sp), xlApp.Cells(AnzZeilen, Sp))
          
          if (not isNull(oSpalte.NumberFormat)) then
            if (oSpalte.NumberFormat = "@") then
              
              'Hier passiert's :-)
              for each Zelle in oSpalte
                ZellInhalt = Zelle.Value
                if (left(ZellInhalt, 1) = "'") then Zelle.Value = mid(ZellInhalt, 2)
              next
              
            end if
          end if
        next
      end if
    end if
    on error goto 0
  end sub
 
 ' interne Funktionen  ----------------------------------------------------------------------------
  
  private sub LadeStartupAddins(xlApp)
    'Alle Add-Ins der beiden Startverzeichnises laden.
    'Parameter:  xlApp ... Existierendes Objekt "Excel.App".

    dim i
    dim oAddins, oAddInFilter, AddinsKeys, AddinFile
    set oAddins      = CreateObject("Scripting.Dictionary")
    set oAddInFilter = CreateObject("Scripting.Dictionary")
    
    oSkript.echo "Startverzeichnis:   '" & xlApp.StartupPath & "'"
    oSkript.echo "Altern. Startverz.: '" & xlApp.AltStartupPath & "'"

    oAddInFilter.Add oAddInFilter.count, xlApp.StartupPath & "\*.xla"
    oAddInFilter.Add oAddInFilter.count, xlApp.AltStartupPath & "\*.xla"
    oAddInFilter.Add oAddInFilter.count, xlApp.StartupPath & "\*.xlam"
    oAddInFilter.Add oAddInFilter.count, xlApp.AltStartupPath & "\*.xlam"

    if (oTools_1.FindeDateien_xMasken(oAddInFilter, oAddins, "", false) = 0) then
      oSkript.echo "Keine Addins in den Startverzeichnissen gefunden."
    else
      AddinsKeys = oAddins.Keys
      For i = 0 To oAddins.Count -1
        AddinFile = AddinsKeys(i)
        oSkript.echo "Lade '" & AddinFile & "'"
        xlApp.Workbooks.open(AddinFile)
      next
      oSkript.echo "Alle Addins der Startverzeichnisse geladen."
    end if
    
    set oAddins      = nothing
    set oAddInFilter = nothing
  end sub
  
  Private function GetXLVorlagen(DateiMaske)
    'Erzeugt eine Liste aller verfügbarer XL-Vorlagen, die in den üblichen 
    'zwei Vorlagen-Verzeichnissen zu finden sind.
    '  Parameter: DateiMaske ... DateiMaske ohne Pfadangabe (mit Wildcards)
    '  Rückgabe = Dateiliste als Dictionary mit Dateinamen als Key.
    on error goto 0
    Dim Anz, oVorlagen, oVorlagenFilter
    set oVorlagen       = CreateObject("Scripting.Dictionary")
    set oVorlagenFilter = CreateObject("Scripting.Dictionary")
    
    oSkript.echo "Suche XL-Vorlagen:      '" & DateiMaske & "'."
    oSkript.echo "Netzwerk-Vorlagenverz.: '" & xlApp.NetworkTemplatesPath & "'."
    oSkript.echo "lokales Vorlagenverz.:  '" & xlApp.TemplatesPath & "'."
    If (xlApp.NetworkTemplatesPath <> "") Then oVorlagenFilter.Add oVorlagenFilter.count, xlApp.NetworkTemplatesPath & "\" & DateiMaske
    If (xlApp.TemplatesPath <> "")        Then oVorlagenFilter.Add oVorlagenFilter.count, xlApp.TemplatesPath        & "\" & DateiMaske

    Anz = oTools_1.FindeDateien_xMasken(oVorlagenFilter, oVorlagen, "", true)

    if (Anz = 0) then
      oSkript.debugecho "Keine Excel-Vorlagen entsprechend Maske '" & DateiMaske & "' in den Vorlagen-Verzeichnissen gefunden."
    else
      oSkript.debugecho Anz & " Excel-Vorlagen in den Vorlagen-Verzeichnissen gefunden."
    end if
    set oVorlagenFilter = nothing
    set GetXLVorlagen = oVorlagen
  End function
 '
end class 

