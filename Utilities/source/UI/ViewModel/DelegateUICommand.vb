
Imports System

Namespace UI.ViewModel
    
    ''' <summary> An ICommand for use in UI elements whose delegates take one parameter of given type. </summary>
     ''' <typeparam name="T1"> Parameter type for CanExecute() predicate. </typeparam>
     ''' <typeparam name="T2"> Parameter type for Execute() action. </typeparam>
    Public Class DelegateUICommand(Of T1, T2)
        Inherits Cinch.SimpleCommand(Of T1, T2)
        
        #Region "Private Fields"
            
            Private ReadOnly _Decoration    As UICommandDecoration = Nothing
            
        #End Region
        
        #Region "Constructors"
            
            ''' <summary> Hides the inherited constructor. </summary>
            Private Sub New()
                MyBase.New(Nothing, Nothing)
            End Sub
            
            ''' <summary> Hides the inherited constructor. </summary>
            Private Sub New(ByVal ExecuteAction As System.Action(Of T2))
                MyBase.New(Nothing, ExecuteAction)
            End Sub
            
            ''' <summary> Hides the inherited constructor. </summary>
            Private Sub New(CanExecutePredicate As Func(Of T1, Boolean), ExecuteAction As Action(Of T2))
                MyBase.New(CanExecutePredicate, ExecuteAction)
            End Sub
            
            ''' <summary> Creates a new command based on the <see cref="DelegateUICommandInfo(Of T1, T2)"/>. </summary>
             ''' <param name="UICommandInfo"> The collected information to build a DelegateUICommand. </param>
            Public Sub New(ByVal UICommandInfo As DelegateUICommandInfo(Of T1, T2))
                MyBase.New(UICommandInfo.CanExecutePredicate, UICommandInfo.ExecuteAction)
                _Decoration = UICommandInfo.Decoration
            End Sub
            
            ''' <summary> Creates a new command that always can execute. </summary>
             ''' <param name="ExecuteAction"> The execution logic. </param>
             ''' <param name="Decoration">    The decoration that should be applied to the bound UI element. </param>
            Public Sub New(ByVal ExecuteAction As Action(Of T2), Decoration As UICommandDecoration)
                MyBase.New(ExecuteAction)
                _Decoration = Decoration
            End Sub
            
            ''' <summary> Creates a new command that only can execute if the provided predicate returns true. </summary>
             ''' <param name="ExecuteAction"> The execution logic. </param>
             ''' <param name="CanExecutePredicate"> The execution status logic. </param>
             ''' <param name="Decoration">    The decoration that should be applied to  the bound UI element. </param>
            Public Sub New(ByVal ExecuteAction As Action(Of T2), ByVal CanExecutePredicate As Func(Of T1, Boolean), Decoration As UICommandDecoration)
                MyBase.New(CanExecutePredicate, ExecuteAction)
                _Decoration = Decoration
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Returns the Command's UI Decoration information. This is for binding to the UI. </summary>
            Public ReadOnly Property Decoration As UICommandDecoration
                Get
                    Return _Decoration
                End Get
            End Property
            
        #End Region
    
    End Class
    
    ''' <summary> A shortcut to create a <see cref="DelegateUICommand(Of Object, Object)"/>. </summary>
    Public Class DelegateUICommand
        Inherits DelegateUICommand(Of Object, Object)
        
        #Region "Constructors"
            
            ''' <summary> Hides the inherited constructor. </summary>
            Private Sub New()
                MyBase.New(Nothing, Nothing)
            End Sub
            
            ''' <summary> Creates a new command based on the <see cref="DelegateUICommandInfo"/>. </summary>
             ''' <param name="UICommandInfo"> The collected information to build a DelegateUICommand. </param>
            Public Sub New(ByVal UICommandInfo As DelegateUICommandInfo)
                MyBase.New(UICommandInfo)
            End Sub
            
            ''' <summary> Creates a new command that always can execute. </summary>
             ''' <param name="ExecuteAction"> The execution logic. </param>
             ''' <param name="Decoration">    The decoration that should be applied to  the bound UI element. </param>
            Public Sub New(ByVal ExecuteAction As Action(Of Object), Decoration As UICommandDecoration)
                MyBase.New(ExecuteAction, Decoration)
            End Sub
            
            ''' <summary> Creates a new command that only can execute if the provided predicate returns true. </summary>
             ''' <param name="ExecuteAction"> The execution logic. </param>
             ''' <param name="CanExecutePredicate"> The execution status logic. </param>
             ''' <param name="Decoration">    The decoration that should be applied to  the bound UI element. </param>
            Public Sub New(ByVal ExecuteAction As Action(Of Object), ByVal CanExecutePredicate As Func(Of Object, Boolean), Decoration As UICommandDecoration)
                MyBase.New(ExecuteAction, CanExecutePredicate, Decoration)
            End Sub
            
        #End Region
        
    End Class
    
End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
