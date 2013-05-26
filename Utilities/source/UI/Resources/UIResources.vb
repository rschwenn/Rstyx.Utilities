
Imports System
Imports System.Windows
Imports System.Collections
Imports System.Collections.Generic

Namespace UI.Resources
    
    ''' <summary> Static properties that provide UI resources (ResourceDictionaries). </summary>
    Public NotInheritable Class UIResources
        
        #Region "Private Fields"
            
            Private Shared Logger                   As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger(My.Settings.UIResources_LoggerName)
            
            Private Shared ReadOnly SyncHandle      As New Object()
            
            Private Shared _Icons                   As ResourceDictionary = Nothing
            Private Shared _IconRectangles          As Dictionary(Of String, System.Windows.Shapes.Rectangle) = Nothing
            
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
                                Dim u As Uri = new Uri(My.Settings.UIResources_IconResourcesUri, UriKind.Relative)
                                Dim tmp As Object  = Application.LoadComponent(u)
                                _Icons = CType(tmp, ResourceDictionary)
                                Logger.logDebug("Icons [Get]: Icon resources initialized.")
                            Catch ex As Exception 
                                Logger.logError(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_ResourceNotFound, My.Settings.UIResources_IconResourcesUri))
                            End Try 
                        End If
                        Return _Icons
                    End SyncLock
                End Get
            End Property
            
            ''' <summary> Returns a Dictionary with all Icon rectangles from <see cref="Icons"/>. Every key is the ResourceKey without the "_IconBrush" suffix. </summary>
            Public Shared ReadOnly Property IconRectangles() As Dictionary(Of String, System.Windows.Shapes.Rectangle)
                Get
                    SyncLock (SyncHandle)
                        If (_IconRectangles Is Nothing) Then
                            Try
                                _IconRectangles = New Dictionary(Of String, System.Windows.Shapes.Rectangle)
                                
                                For Each de As DictionaryEntry in Icons
                                    Dim Rect As System.Windows.Shapes.Rectangle = getIconRectangle(CType(de.Key, String))
                                    If (Rect IsNot Nothing) Then
                                        Dim IconName As String = CType(de.Key, String)
                                        If (IconName.EndsWith(IconBrushSuffix) AndAlso (IconName.Length > IconBrushSuffix.Length)) Then 
                                            IconName = IconName.Substring(0, IconName.Length - IconBrushSuffix.Length)
                                        End If
                                        _IconRectangles.Add(IconName, Rect)
                                    End If
                                Next
                            Catch ex As Exception 
                                Logger.logError(ex, StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.Global_UnexpectedErrorIn, System.Reflection.MethodBase.GetCurrentMethod().Name))
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
            
        #End Region
        
        #Region "Private Members"
            
            ''' <summary> Creates a rectangle filled with an icon of the given resource name from <see cref="Icons"/>. </summary>
             ''' <param name="ResourceName"> Resource name of the DrawingBrush resource in <see cref="Icons"/> (The "_IconBrush" suffix can be omitted). </param>
             ''' <returns>                   A rectangle with an icon of the given name, or Null. </returns>
            Private Shared Function getIconBrush(byVal ResourceName As String) As System.Windows.Media.DrawingBrush
                SyncLock (SyncHandle)
                    Dim RetBrush As System.Windows.Media.DrawingBrush = Nothing
                    Try
                        If (Not ResourceName.EndsWith(IconBrushSuffix)) Then ResourceName &= IconBrushSuffix
                        
                        If (Not Icons.Contains(ResourceName)) Then
                            'Logger.logError(StringUtils.sprintf("getIconRectangle: Das Icon '%s' existiert nicht.", ResourceName))
                        Else
                            If (Icons(ResourceName).GetType().FullName = "System.Windows.Media.DrawingBrush") Then
                                RetBrush = CType(Icons(ResourceName), System.Windows.Media.DrawingBrush)
                                'RetBrush.Stretch = Windows.Media.Stretch.UniformToFill
                            End If
                        End If
                    Catch ex As Exception 
                        Logger.logError(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorCreatingIconDrawingBrush, ResourceName))
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
                            'Logger.logError(StringUtils.sprintf("getIconRectangle: Das Icon '%s' existiert nicht.", ResourceName))
                        Else
                            If (Icons(ResourceName).GetType().FullName = "System.Windows.Media.DrawingBrush") Then
                                RetRectangle = New System.Windows.Shapes.Rectangle()
                                RetRectangle.Fill = CType(Icons(ResourceName), System.Windows.Media.DrawingBrush).Clone()
                                RetRectangle.Stretch = Windows.Media.Stretch.UniformToFill
                            End If
                        End If
                    Catch ex As Exception 
                        Logger.logError(StringUtils.sprintf(Rstyx.Utilities.Resources.Messages.UIResources_ErrorCreatingIconRectangle, ResourceName))
                    End Try 
                    Return RetRectangle
                End SyncLock
            End Function
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
