
Imports System
Imports System.Linq
Imports System.Windows
Imports System.Collections
Imports System.Collections.Generic

Imports Rstyx.Utilities.StringUtils

Namespace UI.Resources
    
    ''' <summary> Static properties that provide UI resources (ResourceDictionaries). </summary>
    Public NotInheritable Class UIResources
        
        #Region "Private Fields"
            
            Private Shared Logger                   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(My.Settings.UIResources_LoggerName)
            
            Private Shared ReadOnly SyncHandle      As New Object()
            
            Private Shared _Icons                   As ResourceDictionary = Nothing
            Private Shared _IconRectangles          As IEnumerable(Of KeyValuePair(Of String, System.Windows.Shapes.Rectangle)) = Nothing
            Private Shared _Styles                  As ResourceDictionary = Nothing
            
            Private Shared ThemePatchesLoaded       As Boolean = False
            Private Shared AppStylesLoaded          As Boolean = False
            
            Private Const IconBrushSuffix           As String = "_IconBrush"
            
        #End Region
        
        #Region "Constructor"
            
            Private Sub New
                'Hides the Constructor
            End Sub
            
        #End Region
        
        #Region "ReadOnly Properties"
            
            ''' <summary> Returns the ResourceDictionary with WPF Icons. </summary>
             ''' <remarks> Example for WPF binding usage: Fill="{Binding Source={x:Static Logging:UIResources.Icons}, Path=[Tango_Penx1_IconBrush]}". </remarks>
            Public Shared ReadOnly Property Icons() As ResourceDictionary
                Get
                    SyncLock (SyncHandle)
                        If (_Icons Is Nothing) Then
                            Try
                                Logger.logDebug("Icons [Get]: Read IconResources.xaml.")
                                Dim u As Uri = New Uri(My.Settings.UIResources_IconResourcesUri, UriKind.Relative)
                                _Icons = CType(Application.LoadComponent(u), ResourceDictionary)
                                Logger.logDebug("Icons [Get]: Icon resources initialized.")
                            Catch ex As Exception 
                                Logger.logError(sprintf(Rstyx.Utilities.Resources.Messages.Global_ResourceNotFound, My.Settings.UIResources_IconResourcesUri))
                            End Try 
                        End If
                        Return _Icons
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns a Dictionary with all Icon rectangles from <see cref="Icons"/>. Every key is the ResourceKey without the "_IconBrush" suffix. </summary>
            Public Shared ReadOnly Property IconRectangles() As IEnumerable(Of KeyValuePair(Of String, System.Windows.Shapes.Rectangle))
                Get
                    SyncLock (SyncHandle)
                        If (_IconRectangles Is Nothing) Then
                            Try
                                Dim tmp_IconRectangles As New Dictionary(Of String, System.Windows.Shapes.Rectangle)
                                
                                For Each de As DictionaryEntry in Icons
                                    Dim Rect As System.Windows.Shapes.Rectangle = getIconRectangle(CType(de.Key, String))
                                    If (Rect IsNot Nothing) Then
                                        Dim IconName As String = CType(de.Key, String)
                                        If (IconName.EndsWith(IconBrushSuffix) AndAlso (IconName.Length > IconBrushSuffix.Length)) Then 
                                            IconName = IconName.Substring(0, IconName.Length - IconBrushSuffix.Length)
                                        End If
                                        tmp_IconRectangles.Add(IconName, Rect)
                                    End If
                                Next
                                _IconRectangles = tmp_IconRectangles.OrderBy(Of String)(Function(ByVal de2) de2.Key)
                            Catch ex As Exception 
                                Logger.logError(ex, sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
                            End Try 
                        End If
                        Return _IconRectangles
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns the brush for an icon of the given name from <see cref="Icons"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the DrawingBrush resource in <see cref="Icons"/> (The "_IconBrush" suffix can be omitted). </param>
             ''' <returns>                   A DrawingBrush for an icon of the given name, or Null. </returns>
            Public Shared ReadOnly Property IconBrush(byVal ResourceName As String) As System.Windows.Media.DrawingBrush
                Get
                    Return getIconBrush(ResourceName)
                End Get
            End Property
            
            ''' <summary> Creates a rectangle filled with an icon of the given name from <see cref="Icons"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the DrawingBrush resource in <see cref="Icons"/> (The "_IconBrush" suffix can be omitted). </param>
             ''' <returns>                   A rectangle with an icon of the given name, or Null. </returns>
            Public Shared ReadOnly Property IconRectangle(byVal ResourceName As String) As System.Windows.Shapes.Rectangle
                Get
                    Return getIconRectangle(ResourceName)
                End Get
            End Property
            
            ''' <summary> Returns the ResourceDictionary with WPF Styles. </summary>
             ''' <remarks> Example for WPF usage inside an XAML object element tag: <c>x:Static Member="RstyxUtils:UIResources.Styles"</c>. </remarks>
            Public Shared ReadOnly Property Styles() As ResourceDictionary
                Get
                    SyncLock (SyncHandle)
                        If (_Styles Is Nothing) Then
                            Try
                                Logger.logDebug("Styles [Get]: Read IconResources.xaml.")
                                Dim u As Uri = New Uri(My.Settings.UIResources_AppStylesUri, UriKind.Relative)
                                _Styles = CType(Application.LoadComponent(u), ResourceDictionary)
                                Logger.logDebug("Styles [Get]: Icon resources initialized.")
                            Catch ex As Exception 
                                Logger.logError(sprintf(Rstyx.Utilities.Resources.Messages.Global_ResourceNotFound, My.Settings.UIResources_AppStylesUri))
                            End Try 
                        End If
                        Return _Styles
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Gets a style from shared resoursec <see cref="Styles"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the Style resource in <see cref="Styles"/>. </param>
             ''' <returns>                   The requested style, or <see langword="null"/>. </returns>
            Public Shared ReadOnly Property Style(byVal ResourceName As String) As System.Windows.Style
                Get
                    Dim RetValue As System.Windows.Style = Nothing
                    If (Styles.Contains(ResourceName)) Then
                        RetValue = Styles.Item(ResourceName)
                    End If
                    Return RetValue
                End Get
            End Property
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Ensures that styles library has been loaded at application resources level. </summary>
             ''' <remarks>
             ''' This should always work, even if WPF is hosted in a none WPF application.
             ''' </remarks>
            Public Shared Sub EnsureAppStylesLoaded()
                Try
                    EnsureApplicationObjectExists()
                    
                    ' Load and merge desired application resources.
                    If (Not AppStylesLoaded) Then
                        
                        Dim u As Uri = New Uri(My.Settings.UIResources_AppStylesUri, UriKind.Relative)
                        Application.Current.Resources.MergedDictionaries.Add(CType(Application.LoadComponent(u), ResourceDictionary))
                        
                        AppStylesLoaded = True
                    End If
                Catch ex As Exception
                    Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorLoadingAppresources, ex.Message))
                    Logger.logDebug(ex.StackTrace)
                End Try
            End Sub
            
            ''' <summary> Ensures that theme patches has been applied at application resources level. </summary>
             ''' <remarks>
             ''' This should always work, even if WPF is hosted in a none WPF application.
             ''' </remarks>
            Public Shared Sub EnsureThemePatchesApplied()
                Try
                    EnsureApplicationObjectExists()
                    
                    ' Load and merge desired application resources.
                    If (Not ThemePatchesLoaded) Then
                        
                        ' General theme.
                        Dim u As Uri = New Uri(My.Settings.UIResources_ThemePatchGeneralUri, UriKind.Relative)
                        Application.Current.Resources.MergedDictionaries.Add(CType(Application.LoadComponent(u), ResourceDictionary))
                        
                        ' For OS greater than Windows 7:
                        Dim OsVersion As Double
                        Double.TryParse(Environment.OSVersion.Version.Major & "." & Environment.OSVersion.Version.Minor, OsVersion)
                        If (OsVersion > 6.1) Then
                            u = New Uri(My.Settings.UIResources_ThemePatchWin8Uri, UriKind.Relative)
                            Application.Current.Resources.MergedDictionaries.Add(CType(Application.LoadComponent(u), ResourceDictionary))
                        End If
                        
                        ThemePatchesLoaded = True
                    End If
                Catch ex As Exception
                    Logger.logWarning(sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorLoadingAppresources, ex.Message))
                    Logger.logDebug(ex.StackTrace)
                End Try
            End Sub
            
        #End Region
        
        #Region "Private Methods"
            
            ''' <summary> Ensures that <c>Application.Current</c> exists (even in if WPF is hosted in a none WPF application). </summary>
             ''' <remarks>
             ''' If <see cref="Application.Current"/> doesn't exist (none WPF application), it will be created
             ''' and it's <see cref="Application.ShutdownMode"/> is set to <see cref="ShutdownMode.OnExplicitShutdown"/>.
             ''' </remarks>
            Private Shared Sub EnsureApplicationObjectExists()
                If (Application.Current Is Nothing) Then
                    Dim tmp As Application = New Application()
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown
                End If
            End Sub
            
            ''' <summary> Creates a rectangle filled with an icon of the given resource name from <see cref="Icons"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the DrawingBrush resource in <see cref="Icons"/> (The "_IconBrush" suffix can be omitted). </param>
             ''' <returns>                   A rectangle with an icon of the given name, or Null. </returns>
            Private Shared Function getIconBrush(byVal ResourceName As String) As System.Windows.Media.DrawingBrush
                SyncLock (SyncHandle)
                    Dim RetBrush As System.Windows.Media.DrawingBrush = Nothing
                    Try
                        If (Not ResourceName.EndsWith(IconBrushSuffix)) Then ResourceName &= IconBrushSuffix
                        
                        If (Not Icons.Contains(ResourceName)) Then
                            'Logger.logError(sprintf("getIconRectangle: Das Icon '%s' existiert nicht.", ResourceName))
                        Else
                            If (Icons(ResourceName).GetType().FullName = "System.Windows.Media.DrawingBrush") Then
                                RetBrush = CType(Icons(ResourceName), System.Windows.Media.DrawingBrush)
                                'RetBrush.Stretch = Windows.Media.Stretch.UniformToFill
                            End If
                        End If
                    Catch ex As Exception 
                        Logger.logError(sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorCreatingIconDrawingBrush, ResourceName))
                    End Try 
                    Return RetBrush
                End SyncLock
            End Function
            
            ''' <summary> Creates a rectangle filled with an icon of the given resource name from <see cref="Icons"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the DrawingBrush resource in <see cref="Icons"/> (The "_IconBrush" suffix can be omitted). </param>
             ''' <returns>                   A rectangle with an icon of the given name, or Null. </returns>
            Private Shared Function getIconRectangle(byVal ResourceName As String) As System.Windows.Shapes.Rectangle
                SyncLock (SyncHandle)
                    Dim RetRectangle As System.Windows.Shapes.Rectangle = Nothing
                    Try
                        If (Not ResourceName.EndsWith(IconBrushSuffix)) Then ResourceName &= IconBrushSuffix
                        
                        If (Not Icons.Contains(ResourceName)) Then
                            'Logger.logError(sprintf("getIconRectangle: Das Icon '%s' existiert nicht.", ResourceName))
                        Else
                            If (Icons(ResourceName).GetType().FullName = "System.Windows.Media.DrawingBrush") Then
                                RetRectangle = New System.Windows.Shapes.Rectangle()
                                RetRectangle.Fill = CType(Icons(ResourceName), System.Windows.Media.DrawingBrush).Clone()
                                RetRectangle.Stretch = Windows.Media.Stretch.UniformToFill
                            End If
                        End If
                    Catch ex As Exception 
                        Logger.logError(sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorCreatingIconRectangle, ResourceName))
                    End Try 
                    Return RetRectangle
                End SyncLock
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
