
Imports System
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports Rstyx.Utilities.IO
Imports Rstyx.Utilities.Math.MathExtensions
Imports Rstyx.Utilities.StringUtils

Namespace Domain
    
    ''' <summary> Constraints to point values that are required for a certain purpose. </summary>
    <Flags>
    Public Enum GeoPointConstraints As Integer
        
        ''' <summary> No constraints. </summary>
        None = 0
        
        ''' <summary> The point's position has to be known, hence X and Y must not be <c>Double.NaN</c>. </summary>
        KnownPosition = 1
        
        ''' <summary> The point's height has to be known, hence Z must not be <c>Double.NaN</c>. </summary>
        KnownHeight = 2
                
        ''' <summary> Values in canted rails system are required. </summary>
         ''' <remarks> This requires a point that implements the <see cref="IPointAtTrackGeometry"/> interface. </remarks>
        KnownCantedRailsSystem = 4
        
        ''' <summary> The point's ID has to be unique in a list of points. </summary>
        UniqueID = 8
        
        ''' <summary> The point's ID has to be unique inside a block of points (see <see cref="Rstyx.Utilities.Domain.IO.TcFileReader.Blocks"/>). </summary>
        UniqueIDPerBlock = 16
        
    End Enum
    
    ''' <summary> Point edit options (i.e. for applying while reading from file). </summary>
    <Flags>
    Public Enum GeoPointEditOptions As Integer
        
        ''' <summary> No editing is applied. </summary>
        None = 0
        
        ''' <summary> Tries to parse <see cref="GeoPoint.ActualCant"/> from the point's <see cref="GeoPoint.Info"/>. </summary>
         ''' <remarks>
         ''' If <see cref="GeoPoint.Kind"/> is <c>None</c> or <c>Rails</c>, then <see cref="GeoPoint.ParseInfoForActualCant()"/> 
         ''' should be invoked to guess point kind. 
         ''' </remarks>
        ParseCantFromInfo = 1
        
        ''' <summary> Tries to guess the point's <see cref="GeoPoint.Kind"/> from it's <see cref="GeoPoint.Info"/>. </summary>
         ''' <remarks>
         ''' If <see cref="GeoPoint.Kind"/> is <c>None</c>, then <see cref="GeoPoint.ParseInfoForKindHints()"/> 
         ''' should be invoked to guess point kind. 
         ''' </remarks>
        GuessAllKindsFromInfo = 2
        
    End Enum
    
    ''' <summary> Supported point kinds. </summary>
    Public Enum GeoPointKind As Integer
        
        ''' <summary> Not a supported point kind. </summary>
        None = 0
        
        ''' <summary> Rails axis. </summary>
        Rails = 1
        
        ''' <summary> Platform edge. </summary>
        Platform = 2
                
        ''' <summary> General fix point. </summary>
        FixPoint = 3
        
        ''' <summary> Rails fix point. </summary>
        RailsFixPoint = 4
        
    End Enum
    
    
    ''' <summary> Representation of a geodetic point including some point info. </summary>
     ''' <remarks></remarks>
    Public Class GeoPoint
        Implements IGeoPoint
        
        #Region "Private Fields"
            
            'Private Shared Logger As Rstyx.LoggingConsole.Logger = Rstyx.LoggingConsole.LogBox.getLogger("Rstyx.Utilities.Domain.GeoPoint")
            
            Private   Shared ReadOnly InfoKindPatterns As Dictionary(Of String, GeoPointKind)
            Private   Shared ReadOnly InfoCantPattern  As String
            
            Protected Shared ReadOnly AttributeNames   As Dictionary(Of String, String)
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                ' Patterns for recognizing point kind from info text.
                InfoCantPattern = "u *=? *([+-]? *[0-9]+)\s*"
                
                InfoKindPatterns = New Dictionary(Of String, GeoPointKind)
                InfoKindPatterns.Add(InfoCantPattern, GeoPointKind.Rails)
                InfoKindPatterns.Add("Gls|Gleis"    , GeoPointKind.Rails)
                InfoKindPatterns.Add("Bst|Bstg"     , GeoPointKind.Platform)
                InfoKindPatterns.Add("PS4|GVP"      , GeoPointKind.RailsFixPoint)
                InfoKindPatterns.Add("PS3|HFP"      , GeoPointKind.FixPoint)
                InfoKindPatterns.Add("PS2|LFP|PPB"  , GeoPointKind.FixPoint)
                InfoKindPatterns.Add("PS1|GPSC"     , GeoPointKind.FixPoint)
                InfoKindPatterns.Add("PS0|NXO|DBRF" , GeoPointKind.FixPoint)
                
                ' Mapping:  property name => attribute name.
                AttributeNames = New Dictionary(Of String, String)
                AttributeNames.Add("MarkTypeAB"        , "VArtAB")
                AttributeNames.Add("TrackPos.TrackNo"  , "StrNr")
                AttributeNames.Add("TrackPos.RailsCode", "StrRi")
                AttributeNames.Add("TrackPos.Kilometer", "StrKm")
                AttributeNames.Add("Km"                , "StrKm")
            End Sub
            
            ''' <summary> Creates a new GeoPoint. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new GeoPoint and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
               Me.GetPropsFromIGeoPoint(SourcePoint)
            End Sub
            
        #End Region
        
        #Region "IGeoPointConversions Members"
            
            ''' <summary> Returns a <see cref="GeoVEPoint"/> initialized with values of the implementing point. </summary>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoVEPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoVEPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoVEPoint() As GeoVEPoint Implements IGeoPointConversions.AsGeoVEPoint
                If (TypeOf Me Is GeoVEPoint) Then
                    Return Me
                Else
                    Return New GeoVEPoint(Me)
                End If
            End Function
            
            ''' <summary> Returns a <see cref="GeoIPoint"/> initialized with values of the implementing point. </summary>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoIPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoIPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoIPoint() As GeoIPoint Implements IGeoPointConversions.AsGeoIPoint
                If (TypeOf Me Is GeoIPoint) Then
                    Return Me
                Else
                    Return New GeoIPoint(Me)
                End If
            End Function
            
            ''' <summary> Returns a <see cref="GeoTcPoint"/> initialized with values of the implementing point. </summary>
             ''' <remarks>
             ''' If the implementing point is already a <see cref="GeoTcPoint"/>, then the same instance will be returned.
             ''' Otherwise a new instance of <see cref="GeoTcPoint"/> will be created.
             ''' </remarks>
            Public Function AsGeoTcPoint() As GeoTcPoint Implements IGeoPointConversions.AsGeoTcPoint
                If (TypeOf Me Is GeoTcPoint) Then
                    Return Me
                Else
                    Return New GeoTcPoint(Me)
                End If
            End Function
            
            ''' <summary> Sets this point's <see cref="IGeoPoint"/> properties from a given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks></remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Protected Sub GetPropsFromIGeoPoint(SourcePoint As IGeoPoint)
                
                If (SourcePoint IsNot Nothing) Then
                    Me.ID              = SourcePoint.ID
                    
                    Me.Attributes      = SourcePoint.Attributes
                    Me.ActualCant      = SourcePoint.ActualCant
                    Me.Info            = SourcePoint.Info
                    Me.HeightInfo      = SourcePoint.HeightInfo
                    Me.Comment         = SourcePoint.Comment
                    Me.Kind            = SourcePoint.Kind
                    Me.KindText        = SourcePoint.KindText
                    Me.MarkType        = SourcePoint.MarkType
                    Me.MarkHints       = SourcePoint.MarkHints
                    Me.ObjectKey       = SourcePoint.ObjectKey
                    Me.Job             = SourcePoint.Job
                    Me.TimeStamp       = SourcePoint.TimeStamp
                    
                    Me.X               = SourcePoint.X
                    Me.Y               = SourcePoint.Y
                    Me.Z               = SourcePoint.Z
                    Me.mp              = SourcePoint.mp
                    Me.mh              = SourcePoint.mh
                    Me.wp              = SourcePoint.wp
                    Me.wh              = SourcePoint.wh
                    Me.sp              = SourcePoint.sp
                    Me.sh              = SourcePoint.sh
                    Me.CoordSys        = SourcePoint.CoordSys
                    Me.HeightSys       = SourcePoint.HeightSys
                    
                    Me.SourcePath      = SourcePoint.SourcePath
                    Me.SourceLineNo    = SourcePoint.SourceLineNo
                End If
            End Sub
            
        #End Region
        
        #Region "IIdentifiable Members"
            
            Protected _ID As String = Nothing
            
            ''' <summary> The point ID (It will be verified while setting it). </summary>
             ''' <remarks> The setter fails if given ID isn't valid for this point type. </remarks>
             ''' <exception cref="InvalidIDException"> The value to set as ID isn't a valid ID for this point. </exception>
            Public Property ID() As String Implements IIdentifiable(Of String).ID
                Get
                    Return _ID
                End Get
                Set(value As String)
                    _ID = Me.ParseID(value)
                End Set
            End Property
            
        #End Region
        
        #Region "IGeoPointInfo Members"
            
            Private _Attributes As Dictionary(Of String, String)
            
            ''' <summary> A bunch of free attributes (key/value pairs). </summary>
             ''' <remarks>
             ''' The Getter never returns <see langword="null"/> but rather an empty dictionary.
             ''' </remarks>
            Public Property Attributes()    As Dictionary(Of String, String) Implements IGeoPointInfo.Attributes
                Get
                    If (_Attributes Is Nothing) Then _Attributes = New Dictionary(Of String, String)
                    Return _Attributes
                End Get
            Set (value As Dictionary(Of String, String))
                _Attributes = value
            End Set
            End Property
            
            ''' <inheritdoc/>
            Public Property ActualCant()    As Double = Double.NaN Implements IGeoPointInfo.ActualCant
            
            ''' <inheritdoc/>
            Public Property Info()          As String = String.Empty Implements IGeoPointInfo.Info
            
            ''' <inheritdoc/>
            Public Property HeightInfo()    As String = String.Empty Implements IGeoPointInfo.HeightInfo
            
            ''' <inheritdoc/>
            Public Property Comment()       As String = String.Empty Implements IGeoPointInfo.Comment
            
            ''' <inheritdoc/>
            Public Property Kind()          As GeoPointKind = GeoPointKind.None Implements IGeoPointInfo.Kind
            
            ''' <inheritdoc/>
            Public Property KindText()      As String = String.Empty Implements IGeoPointInfo.KindText
            
            ''' <inheritdoc/>
            Public Property MarkType()      As String = String.Empty Implements IGeoPointInfo.MarkType
            
            ''' <inheritdoc/>
            Public Property MarkHints()     As String = String.Empty Implements IGeoPointInfo.MarkHints
            
            ''' <inheritdoc/>
            Public Property ObjectKey()     As String = String.Empty Implements IGeoPointInfo.ObjectKey
            
            ''' <inheritdoc/>
            Public Property Job()           As String = String.Empty Implements IGeoPointInfo.Job
            
            ''' <inheritdoc/>
            Public Property TimeStamp       As Nullable(Of DateTime) = Nothing Implements IGeoPointInfo.TimeStamp
            
        #End Region
        
        #Region "ICartesianCoordinates3D Members"
            
            ''' <inheritdoc/>
            Public Property X()             As Double = Double.NaN   Implements ICartesianCoordinates3D.X
            
            ''' <inheritdoc/>
            Public Property Y()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Y
            
            ''' <inheritdoc/>
            Public Property Z()             As Double = Double.NaN   Implements ICartesianCoordinates3D.Z
            
            ''' <inheritdoc/>
            Public Property mp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mp
            
            ''' <inheritdoc/>
            Public Property mh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.mh
            
            ''' <inheritdoc/>
            Public Property wp()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wp
            
            ''' <inheritdoc/>
            Public Property wh()            As Double = Double.NaN   Implements ICartesianCoordinates3D.wh
            
            ''' <inheritdoc/>
            Public Property sp()            As String = String.Empty Implements ICartesianCoordinates3D.sp
            
            ''' <inheritdoc/>
            Public Property sh()            As String = String.Empty Implements ICartesianCoordinates3D.sh
            
            ''' <inheritdoc/>
            Public Property CoordSys()      As String = String.Empty Implements ICartesianCoordinates3D.CoordSys
            
            ''' <inheritdoc/>
            Public Property HeightSys()     As String = String.Empty Implements ICartesianCoordinates3D.HeightSys
            
        #End Region
        
        #Region "IFileSource Members"
            
            ''' <inheritdoc/>
            Public Property SourcePath()    As String = Nothing Implements IFileSource.SourcePath
            
            ''' <inheritdoc/>
            Public Property SourceLineNo()  As Long = 0 Implements IFileSource.SourceLineNo
            
        #End Region
        
        #Region "ID Validation"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' <para>
             ''' For the base class <see cref="GeoPoint"/>, this method returns the trimmed <paramref name="TargetID"/>
             ''' and validation only complains about the ID, if it is <see langword="null"/>, empty or contains whitespace only.
             ''' Derived classes should override this behavior to implement more restrictive constraints.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> isn't a valid ID for this point. </exception>
            Protected Overridable Function ParseID(TargetID As String) As String
                
                If (TargetID.IsEmptyOrWhiteSpace()) Then
                    Throw New InvalidIDException(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPointID)
                End If
                
                Return TargetID.Trim()
            End Function
            
        #End Region
        
        #Region "Constraints Verifying"
            
            ''' <summary> Verifies that this point fulfills all given <paramref name="Constraints"/>. </summary>
             ''' <param name="Constraints"> A set of <see cref="GeoPointConstraints"/> to verify for this point. </param>
             ''' <remarks>
             ''' If any of the <see cref="GeoPointConstraints"/> is violated, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' Therefore, <see cref="GeoPoint.SourceLineNo"/> and <see cref="GeoPoint.SourcePath"/> should be set for this point.
             ''' </remarks>
             ''' <exception cref="ParseException"> At least one constraint is violated. </exception>
            Public Sub VerifyConstraints(Constraints As GeoPointConstraints)
                Me.VerifyConstraints(Constraints, Nothing, Nothing, Nothing)
            End Sub
            
            ''' <summary> Verifies that this point fulfills all given <paramref name="Constraints"/>. </summary>
             ''' <param name="Constraints"> A set of <see cref="GeoPointConstraints"/> to verify for this point. </param>
             ''' <param name="FieldX">  The parsed data field of X coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldY">  The parsed data field of Y coordinate. May be <see langword="null"/>. </param>
             ''' <param name="FieldZ">  The parsed data field of Z coordinate. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' If any of the <see cref="GeoPointConstraints"/> is violated, a <see cref="ParseException"/> will be thrown.
             ''' In this case, a <see cref="ParseError"/> will be created and delivered with the <see cref="ParseException"/>.
             ''' The <see cref="ParseError"/> will contain error source information if available.
             ''' Therefore, <see cref="GeoPoint.SourceLineNo"/> and <see cref="GeoPoint.SourcePath"/> should be set for this point.
             ''' The Datafields may be given in order to highlight violation position exactly in source file.
             ''' </remarks>
             ''' <exception cref="ParseException"> At least one constraint is violated. </exception>
            Public Sub VerifyConstraints(Constraints As GeoPointConstraints,
                                         FieldX      As DataField(Of Double),
                                         FieldY      As DataField(Of Double),
                                         FieldZ      As DataField(Of Double)
                                        )
                Dim PointID  As String  = Me.ID
                Dim StartCol As Integer = 0
                Dim EndCol   As Integer = 0
                
                ' Position missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownPosition)) Then
                    If (Double.IsNaN(Me.X) OrElse Double.IsNaN(Me.Y)) Then
                        If (Me.SourceLineNo > 0) Then
                            If ((FieldX IsNot Nothing) AndAlso (FieldY IsNot Nothing) AndAlso FieldX.HasSource AndAlso FieldY.HasSource) Then
                                If (FieldX.Source.Column < FieldY.Source.Column) Then
                                    StartCol = FieldX.Source.Column
                                    EndCol   = FieldY.Source.Column + FieldY.Source.Length
                                Else
                                    StartCol = FieldY.Source.Column
                                    EndCol   = FieldX.Source.Column + FieldX.Source.Length
                                End If
                            End If
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Me.SourceLineNo, StartCol, EndCol,
                                                                    sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID),
                                                                    Nothing,
                                                                    Me.SourcePath
                                                                   ))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingPosition, PointID)))
                        End If
                    End If
                End If
                
                ' Heigt missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownHeight)) Then
                    If (Double.IsNaN(Me.Z)) Then
                        Throw New ParseException(ParseError.Create(ParseErrorLevel.[Error],
                                                                   Me.SourceLineNo,
                                                                   FieldZ,
                                                                   sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingHeight, PointID),
                                                                   Nothing,
                                                                   Me.SourcePath
                                                                  ))
                    End If
                End If
                                    
                ' Canted Rails System missing.
                If (Constraints.HasFlag(GeoPointConstraints.KnownCantedRailsSystem)) Then
                    
                    Dim Missing As Boolean = False
                    Dim Hints   As String  = Nothing
                    
                    If (Not TypeOf Me Is IPointAtTrackGeometry) Then
                        Missing = True
                        Hints   = Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingTrackValues
                    Else
                        Dim tp As GeoTcPoint = Me.AsGeoTcPoint()
                        
                        If (Double.IsNaN(tp.QG) OrElse Double.IsNaN(tp.HG)) Then
                            Missing = True
                            If (Double.IsNaN(tp.HSOK)) Then
                                Hints = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_HSOK)
                            ElseIf (Double.IsNaN(tp.Ueb)) Then
                                Hints = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_Ueb)
                            ElseIf (Double.IsNaN(tp.Ra)) Then
                                Hints = sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingField, Rstyx.Utilities.Resources.Messages.Domain_Label_Ra)
                            ElseIf (tp.Ra.EqualsTolerance(0, 0.001)) Then
                                Hints = Rstyx.Utilities.Resources.Messages.GeoPointConstraints_Hint_MissingCantSign
                            End If
                        End If
                    End If
                    
                    If (Missing) Then
                        If (Me.SourceLineNo > 0) Then
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                    Me.SourceLineNo, 0, 0,
                                                                    sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingCantedRailsSystem, PointID),
                                                                    Hints,
                                                                    Me.SourcePath
                                                                   ))
                        Else
                            Throw New ParseException(New ParseError(ParseErrorLevel.[Error], sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_MissingCantedRailsSystem, PointID)))
                        End If
                    End If
                End If
            End Sub
            
        #End Region
        
        #Region "Public Methods"
            
            ''' <summary> Guesses the point kind from <see cref="GeoPoint.Info"/> property (somewhat heuristic). </summary>
             ''' <remarks>
             ''' <para>
             ''' The following patterns and tokens somewhere in <see cref="GeoPoint.Info"/> lead to guessing the point kind: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Pattern</b>  </term>  <description> Point Kind </description></listheader>
             ''' <item> <term> u[=]?[+-]?[0-9]+      </term>  <description> Actual rails with actual cant      </description></item>
             ''' <item> <term> Gls, Gleis            </term>  <description> Actual rails (without actual cant) </description></item>
             ''' <item> <term> Bstg, Bst             </term>  <description> Platform                           </description></item>
             ''' <item> <term> PS4, GVP              </term>  <description> Rails fix point                    </description></item>
             ''' <item> <term> PS3, HFP              </term>  <description> Other fix point                    </description></item>
             ''' <item> <term> PS2, LFP, PPB         </term>  <description> Other fix point                    </description></item>
             ''' <item> <term> PS1, GPSC             </term>  <description> Other fix point                    </description></item>
             ''' <item> <term> PS0, NXO, DBRF, DBREF </term>  <description> Other fix point                    </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' When an actual rails point has been determined by a cant pattern, 
             ''' this cant pattern (inclusive trailing whitespace) will be removed from info text, 
             ''' because it will be added to the code part of PointInfoText
             ''' when the point is written to a ipkt file. All other tokens are kept in info text.
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.Kind"/>       </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.ActualCant"/> </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Info"/>       </term>  <description> A found cant pattern will be removed. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub ParseInfoForKindHints()
                
                Me.Kind       = GeoPointKind.None
                Me.ActualCant = Double.NaN
                
                If (Me.Info.IsNotEmptyOrWhiteSpace()) Then
                    
                    ' Test patterns against Me.Info (for patterns: see Shared Sub New()).
                    For Each kvp As KeyValuePair(Of String, GeoPointKind) In InfoKindPatterns
                        
                        Dim oMatch As Match = Regex.Match(Me.Info, kvp.Key, RegexOptions.IgnoreCase)
                        
                        If (oMatch.Success) Then
                            
                            Me.Kind = kvp.Value
                            
                            ' Rails: set actual cant and remove cant pattern from info.
                            If ((kvp.Value = GeoPointKind.Rails) AndAlso (oMatch.Groups.Count > 1)) Then
                                Me.ActualCant = CDbl(oMatch.Groups(1).Value.Replace(" ", String.Empty)) / 1000
                                Me.Info       = Me.Info.Remove(oMatch.Index, oMatch.Length).TrimEnd()
                            End If
                            
                            Exit For
                        End If
                    Next
                End If
            End Sub
            
            ''' <summary> Parses actual cant from <see cref="GeoPoint.Info"/> property. </summary>
             ''' <remarks>
             ''' <para>
             ''' The cant is recognizes by this pattern in <see cref="GeoPoint.Info"/>:  u[=]?[+-]?[0-9]+
             ''' </para>
             ''' <para>
             ''' When cant has been found, 
             ''' this cant pattern (inclusive trailing whitespace) will be removed from info text, 
             ''' because it will be added to the code part of PointInfoText
             ''' when the point is written to a ipkt file. All other tokens are kept in info text.
             ''' </para>
             ''' <para>
             ''' This method changes the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Action </description></listheader>
             ''' <item> <term> <see cref="GeoPoint.ActualCant"/> </term>  <description> Cleared and maybe set. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Info"/>       </term>  <description> A found cant pattern will be removed. </description></item>
             ''' <item> <term> <see cref="GeoPoint.Kind"/>       </term>  <description> Only if cant has been found, kind will be changed to <c>Rails</c>. </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub ParseInfoForActualCant()
                
                Me.ActualCant = Double.NaN
                
                If (Me.Info.IsNotEmptyOrWhiteSpace()) Then
                    
                    Dim oMatch As Match = Regex.Match(Me.Info, InfoCantPattern, RegexOptions.IgnoreCase)
                    
                    If (oMatch.Success) Then
                        
                        ' Rails: set actual cant and remove cant pattern from info.
                        If (oMatch.Groups.Count > 1) Then
                            Me.ActualCant = CDbl(oMatch.Groups(1).Value.Replace(" ", String.Empty)) / 1000
                            Me.Info       = Me.Info.Remove(oMatch.Index, oMatch.Length).TrimEnd()
                            Me.Kind       = GeoPointKind.Rails
                        End If
                        
                    End If
                End If
            End Sub
            
            ''' <summary>
             ''' Gets the value of an attribute which name is determined by the given property name
             ''' and the <see cref="AttributeNames"/> assignment table.
             ''' </summary>
             ''' <param name="PropertyName"> The name of the target property. May be <see langword="null"/> </param>
             ''' <returns> The attribute's string value. May be <see langword="null"/> </returns>
             ''' <remarks>
             ''' If <paramref name="PropertyName"/> is a key in <see cref="AttributeNames"/> the matching dictionary value
             ''' is the attribute name to look for. If this attribute exists in <see cref="Attributes"/>, 
             ''' it's value will be returned.
             ''' </remarks>
            Public Function GetAttValueByPropertyName(PropertyName As String) As String
                
                Dim AttValue As String = Nothing
                
                If (PropertyName.IsNotEmptyOrWhiteSpace()) Then
                    If (AttributeNames.ContainsKey(PropertyName)) Then
                        Dim AttName As String = AttributeNames(PropertyName)
                        If (AttName.IsNotEmptyOrWhiteSpace()) Then
                            If (Me.Attributes.ContainsKey(AttName)) Then
                                AttValue = Me.Attributes(AttName)
                            End If
                        End If
                    End If
                End If
                
                Return AttValue
            End Function
            
        #End Region
            
        #Region "Overrides"
            
            ''' <summary> Returns point ID and info. </summary>
            Public Overrides Function ToString() As String
                Return Me.ID & "  (" & Me.Info & ")"
            End Function
            
        #End Region
        
    End Class

End Namespace

' for jEdit:  :collapseFolds=3::tabSize=4::tabSize=4:
