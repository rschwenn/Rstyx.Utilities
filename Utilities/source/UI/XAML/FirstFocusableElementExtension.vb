'
'Imports System
'Imports System.Collections.Generic
'Imports System.Linq
'Imports System.Reflection
'Imports System.Windows
'Imports System.Windows.Markup
'Imports System.Windows.Media
'Imports System.ComponentModel
'Imports System.Windows.Input
'
'
'Namespace UI.XAML
'    
'    ''' <summary>
'    ''' This markup extension locates the first focusable child and returns it.
'    ''' It is intended to be used with FocusManager.FocusedElement:
'    ''' (Window ... FocusManager.FocusedElement={ft:FirstFocusedElement} /)
'    ''' </summary>
'    ''' <remarks> Found at http://www.julmar.com/blog/mark/?p=50 (many thanks!) and converted to VisualBasic </remarks>
'    Public Class FirstFocusedElementExtension
'        Inherits MarkupExtension
'        ''' <summary>
'        ''' Unhook the handler after it has set focus to the element the first time
'        ''' </summary>
'        Public Property OneTime() As Boolean
'            Get
'                Return m_OneTime
'            End Get
'            Set
'                m_OneTime = Value
'            End Set
'        End Property
'        Private m_OneTime As Boolean
'        
'        ''' <summary> Constructor </summary>
'        Public Sub New()
'            OneTime = True
'        End Sub
'        
'        ''' <summary> This method locates the first focusable + visible element we can change focus to. </summary>
'        ''' <param name="serviceProvider"> IServiceProvider from XAML </param>
'        ''' <returns>                      Focusable Element or <c>Null</c> </returns>
'        Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
'            ' Ignore if in design mode
'            If CBool(DesignerProperties.IsInDesignModeProperty.GetMetadata(GetType(DependencyObject)).DefaultValue) Then
'                Return Nothing
'            End If
'            
'            ' Get the IProvideValue interface which gives us access to the target property 
'            ' and object.  Note that MSDN calls this interface "internal" but it's necessary
'            ' here because we need to know what property we are assigning to.
'            Dim pvt = TryCast(serviceProvider.GetService(GetType(IProvideValueTarget)), IProvideValueTarget)
'            If pvt IsNot Nothing Then
'                Dim fe = TryCast(pvt.TargetObject, FrameworkElement)
'                Dim targetProperty As Object = pvt.TargetProperty
'                If fe IsNot Nothing Then
'                    ' If the element isn't loaded yet, then wait for it.
'                    If Not fe.IsLoaded Then
'                        Dim deferredFocusHookup As RoutedEventHandler = Nothing
'                        deferredFocusHookup = Sub() 
'                        ' Ignore if the element is now loaded but not
'                        ' visible -- this happens for things like TabItem.
'                        ' Instead, we'll wait until the item becomes visible and
'                        ' then set focus.
'                        If fe.IsVisible = False Then
'                            Return
'                        End If
'                        
'                        ' Look for the first focusable leaf child and set the property
'                        Dim ie As IInputElement = GetLeafFocusableChild(fe)
'                        If TypeOf targetProperty Is DependencyProperty Then
'                            ' Specific case where we are setting focused element.
'                            ' We really need to set this property onto the focus scope, 
'                            ' so we'll use UIElement.Focus() which will do exactly that.
'                            If targetProperty Is FocusManager.FocusedElementProperty Then
'                                ie.Focus()
'                            Else
'                                ' Being assigned to some other property - just assign it.
'                                fe.SetValue(DirectCast(targetProperty, DependencyProperty), ie)
'                            End If
'                        ' Simple property assignment through reflection.
'                        ElseIf TypeOf targetProperty Is PropertyInfo Then
'                            Dim pi = DirectCast(targetProperty, PropertyInfo)
'                            pi.SetValue(fe, ie, Nothing)
'                        End If
'                        
'                        ' Unhook the handler if we are supposed to.
'                        If OneTime Then
'                            RemoveHandler fe.Loaded, deferredFocusHookup
'                        End If
'                        
'                        End Sub
'                        
'                        ' Wait for the element to load
'                        AddHandler fe.Loaded, deferredFocusHookup
'                    Else
'                        Return GetLeafFocusableChild(fe)
'                    End If
'                End If
'            End If
'            
'            Return Nothing
'        End Function
'        
'        ''' <summary>
'        ''' Locate the first real focusable child.  We keep going down
'        ''' the visual tree until we hit a leaf node.
'        ''' </summary>
'        ''' <param name="fe"></param>
'        ''' <returns></returns>
'        Private Shared Function GetLeafFocusableChild(fe As IInputElement) As IInputElement
'            Dim ie As IInputElement = GetFirstFocusableChild(fe), final As IInputElement = ie
'            While final IsNot Nothing
'                ie = final
'                final = GetFirstFocusableChild(final)
'            End While
'        
'            Return ie
'        End Function
'        
'        ''' <summary>
'        ''' This searches the Visual Tree looking for a valid child which can have focus.
'        ''' </summary>
'        ''' <param name="fe"></param>
'        ''' <returns></returns>
'        Private Shared Function GetFirstFocusableChild(fe As IInputElement) As IInputElement
'            Dim dpo = TryCast(fe, DependencyObject)
'            Return IIf(dpo Is Nothing, Nothing, (From vc In EnumerateVisualTree(dpo, Function(c) Not FocusManager.GetIsFocusScope(c))Let iic = TryCast(vc, IInputElement) Where iic IsNot Nothing AndAlso iic.Focusable AndAlso iic.IsEnabled AndAlso (Not (TypeOf iic Is FrameworkElement) OrElse (DirectCast(iic, FrameworkElement).IsVisible))).FirstOrDefault())
'        End Function
'        
'        ''' <summary>
'        ''' A simple iterator method to expose the visual tree to LINQ
'        ''' </summary>
'        ''' <param name="start"></param>
'        ''' <param name="eval"></param>
'        ''' <returns></returns>
'        Private Shared Function EnumerateVisualTree(Of T As DependencyObject)(start As T, eval As Predicate(Of T)) As IEnumerable(Of T)
'            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(start) - 1
'                Dim child = TryCast(VisualTreeHelper.GetChild(start, i), T)
'                If child IsNot Nothing AndAlso (eval IsNot Nothing AndAlso Eval(child)) Then
'                    yield Return child
'                    For Each childOfChild As var In EnumerateVisualTree(child, eval)
'                        yield Return childOfChild
'                    Next
'                End If
'            Next
'        End Function
'        
'    End Class
'    
'End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::indentSize=4:
