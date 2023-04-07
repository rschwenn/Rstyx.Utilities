
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Math
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities.StringUtils
Imports Rstyx.Utilities.Math
Imports Rstyx.Utilities.IO

Namespace Domain
    
    ''' <summary> One single iPoint. </summary>
    Public Class GeoIPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            Private Shared ReadOnly MaxIDLength     As Integer = 20
            Private Shared ReadOnly AttSeparator    As Char    = "|"
            
        #End Region
        
        #Region "Public Fields"
            
            Public Shared ReadOnly DefaultCoordType   As String = "YXZ"
            Public Shared ReadOnly iTC_Pattern        As String
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Static initializations. </summary>
            Shared Sub New()
                    
                ' Regular Expression pattern to recognize a text as iGeo "iTrassen-Codierung".
                Dim RegExDecimal As String = " *=? *([+-]?([0-9]*[.])?[0-9]+)"
                
                iTC_Pattern  = "^ *"
                iTC_Pattern &= "(\w)?"            ' PointKindAB
                
                iTC_Pattern &= "("                ' Start attributes

                iTC_Pattern &= "(-b)"             ' Platform
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-v)([0-9]+)?"    ' Rails fixpoint
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-f)([0-9]+)?"    ' Fixpoint
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-s1)"            ' Rail point 1
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-s2)"            ' Rail point 2
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-m1)"            ' Measure point 1
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-m2)"            ' Measure point 2
                iTC_Pattern &= "|"
                
                ' Rails may have and are recognized by several attributes in arbitrary order:
                ' - 3 different attributes: relative and absolute cant, track gauge.
                ' - 2 attributes each in 2 different notations:
                '   - Absolute cant is "-iueb*" or "-ueb*"
                '   - Relative cant is "-iu*"   or "-u*"
                iTC_Pattern &= "(-i?ueb)" & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?u)"   & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-sp)"    & RegExDecimal
                iTC_Pattern &= "|"

                iTC_Pattern &= "(-i?ueb)" & RegExDecimal & " *(-i?u)"   & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?ueb)" & RegExDecimal & " *(-sp)"    & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?u)"   & RegExDecimal & " *(-i?ueb)" & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?u)"   & RegExDecimal & " *(-sp)"    & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-sp)"    & RegExDecimal & " *(-i?u)"   & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-sp)"    & RegExDecimal & " *(-i?ueb)" & RegExDecimal
                iTC_Pattern &= "|"

                iTC_Pattern &= "(-i?ueb)" & RegExDecimal & " *(-i?u)"   & RegExDecimal & " *(-sp)"    & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?ueb)" & RegExDecimal & " *(-sp)"    & RegExDecimal & " *(-i?u)"   & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?u)"   & RegExDecimal & " *(-i?ueb)" & RegExDecimal & " *(-sp)"    & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i?u)"   & RegExDecimal & " *(-sp)"    & RegExDecimal & " *(-i?ueb)" & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-sp)"    & RegExDecimal & " *(-i?ueb)" & RegExDecimal & " *(-i?u)"   & RegExDecimal
                iTC_Pattern &= "|"
                iTC_Pattern &= "(-sp)"    & RegExDecimal & " *(-i?u)"   & RegExDecimal & " *(-i?ueb)" & RegExDecimal

                iTC_Pattern &= "|"
                iTC_Pattern &= "(-i)"             ' Rails.  In fact, in iGeo this is meant generally as *actual position*.
                
                iTC_Pattern &= ")?"               ' End attributes
                
                iTC_Pattern &= " *((#) ?(.*))?"   ' Comment
                iTC_Pattern &= "$"
            End Sub
            
            ''' <summary> Creates a new, empty <see cref="GeoIPoint"/>. </summary>
            Public Sub New()
                PropertyAttributes.Add("MarkTypeAB" , Rstyx.Utilities.Resources.Messages.Domain_AttName_MarkTypeAB)   ' "VArtAB"

                PropertyAttributes.Add("AttKey1"    , Rstyx.Utilities.Resources.Messages.Domain_AttName_AttKey1)      ' "CodeAtt1"
                PropertyAttributes.Add("AttKey2"    , Rstyx.Utilities.Resources.Messages.Domain_AttName_AttKey2)      ' "CodeAtt2"
                PropertyAttributes.Add("AttValue1"  , Rstyx.Utilities.Resources.Messages.Domain_AttName_AttValue1)    ' "Att1"
                PropertyAttributes.Add("AttValue2"  , Rstyx.Utilities.Resources.Messages.Domain_AttName_AttValue2)    ' "Att2"
                PropertyAttributes.Add("CalcCode"   , Rstyx.Utilities.Resources.Messages.Domain_AttName_CalcCode)     ' "CodeBC"
                PropertyAttributes.Add("CoordType"  , Rstyx.Utilities.Resources.Messages.Domain_AttName_CoordType)    ' "KooKz"
                PropertyAttributes.Add("Flags"      , Rstyx.Utilities.Resources.Messages.Domain_AttName_Flags)        ' "Flags"
                PropertyAttributes.Add("GraficsCode", Rstyx.Utilities.Resources.Messages.Domain_AttName_GraficsCode)  ' "CodeGC"
                PropertyAttributes.Add("GraficsDim" , Rstyx.Utilities.Resources.Messages.Domain_AttName_GraficsDim)   ' "CodeGDim"
                PropertyAttributes.Add("GraficsEcc" , Rstyx.Utilities.Resources.Messages.Domain_AttName_GraficsEcc)   ' "CodeGExz"
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoIPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks> For details see <see cref="GetPropsFromIGeoPoint"/>. </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                
                Me.New()
                Me.GetPropsFromIGeoPoint(SourcePoint)
            End Sub
            
        #End Region
        
        #Region "Properties"
            
            ''' <summary> Calculation Code. </summary>
            Public Property CalcCode    As String = Nothing
            
            ''' <summary> Grafics Code. </summary>
            Public Property GraficsCode As String = Nothing
            
            ''' <summary> Grafics Dimension. </summary>
            Public Property GraficsDim  As Double = Double.NaN
            
            ''' <summary> Grafics Eccentricity. </summary>
            Public Property GraficsEcc  As Double = Double.NaN
            
            ''' <summary> Type of Coordinates, i.e. "YXZ". </summary>
            Public Property CoordType   As String = Nothing
            
            ''' <summary> Flags. </summary>
            Public Property Flags       As String = Nothing
            
            ''' <summary> Key of Attribute 1. </summary>
            Public Property AttKey1     As String = Nothing
            
            ''' <summary> Value of Attribute 1. </summary>
            Public Property AttValue1   As String = Nothing
            
            ''' <summary> Key of Attribute 2. </summary>
            Public Property AttKey2     As String = Nothing
            
            ''' <summary> Value of Attribute 2. </summary>
            Public Property AttValue2   As String = Nothing
            
            ''' <summary> Special mark type (iGeo Trassen-Absteckbuch). </summary>
            Public Property MarkTypeAB  As String = Nothing
            
        #End Region
        
        #Region "Free Data Text - Parsing and Creating"
            
            ''' <summary> Creates a FreeData text for ipkt file, containing attributes and comment. </summary>
             ''' <returns> The FreeData text for ipkt file. </returns>
            Public Function CreateFreeDataText() As String
                
                Dim FreeDataText As String = String.Empty
                                
                ' Attributes.
                If (Me.Attributes?.Count > 0) Then
                    Dim AttString   As String
                    Dim AttName     As String
                    Dim AttNames()  As String = Me.Attributes.Keys.ToArray()
                    
                    ' Format certain attribut values for output.
                    For i As Integer = 0 To AttNames.Count - 1
                        AttName   = AttNames(i)
                        AttString = Me.Attributes(AttName)
                        Select Case AttName
                            Case Rstyx.Utilities.Resources.Messages.Domain_AttName_KindText : Me.Attributes(AttName) = AttString.PadRight(4)
                            Case Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackNo  : Me.Attributes(AttName) = AttString.PadLeft(4)
                            Case Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackKm  : Me.Attributes(AttName) = sprintf("%+15s", New Kilometer(AttString).ToKilometerNotation(4, " "))
                        End Select
                    Next

                    ' All attributes ordered by name.
                    'For Each kvp As KeyValuePair(Of String, String) In Me.Attributes.OrderBy(Of String)(Function(ByVal kvp2) kvp2.Key)
                    '    AttString &= " " & kvp.Key & AttSeparator & kvp.Value & AttSeparator
                    'Next

                    ' First certain known attributes in given order.
                    Dim FirstAttributes As New Dictionary(Of String, Integer)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_KindText,       1)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackNo,        2)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackRailsCode, 3)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_TrackKm,        4)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_CoordSys,       5)
                    FirstAttributes.Add(Rstyx.Utilities.Resources.Messages.Domain_AttName_HeightSys,      6)
                    AttString = String.Empty
                    For Each kvp As KeyValuePair(Of String, Integer) In FirstAttributes.OrderBy(Of Integer)(Function(ByVal kvp2) kvp2.Value)
                        AttName = kvp.Key
                        If (Me.Attributes.ContainsKey(AttName)) Then
                            AttString &= " " & AttName & AttSeparator & Me.Attributes(AttName) & AttSeparator
                        End If
                    Next

                    ' Remaining Attributes.
                    For Each kvp As KeyValuePair(Of String, String) In Me.Attributes.OrderBy(Of String)(Function(ByVal kvp2) kvp2.Key)
                        If (Not FirstAttributes.ContainsKey(kvp.Key)) Then
                            AttString &= " " & kvp.Key & AttSeparator & kvp.Value & AttSeparator
                        End If
                    Next

                    FreeDataText &= If((AttString.Length > 0), AttString.Substring(1), AttString)
                End If
                
                ' Comment.
                If (Me.Comment.IsNotEmptyOrWhiteSpace()) Then
                    FreeDataText &= Me.Comment
                End If
                
                Return FreeDataText
            End Function
            
            ''' <summary> A given String will be parsed for attributes and a comment. May be <see langword="null"/>. </summary>
             ''' <param name="FreeDataText"> The string to parse. </param>
             ''' <remarks>
             ''' <para>
             ''' <paramref name="FreeDataText"/> is required to look <b>like in ipkt, A0, A1</b>: <code>AttName|AttValue|AttName|AttValue|comment</code>. 
             ''' There may be zero, one or more attributes (name/value pairs). The attribute names are trimmed.
             ''' </para>
             ''' <para>
             ''' This method clears and sets the <see cref="GeoPoint.Attributes"/> and <see cref="GeoPoint.Comment"/> properties.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> At least one found attribute has an empty name. </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> At least two found attributes have the same name. </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> Any other Exception will be wrapped by a <see cref="Rstyx.Utilities.IO.ParseException"/>. </exception>
            Public Sub ParseFreeData(FreeDataText As String)
                Me.ParseFreeData(FreeDataText, Nothing)
            End Sub
            
            ''' <summary> A given String will be parsed for attributes and a comment. May be <see langword="null"/>. </summary>
             ''' <param name="FreeDataText"> The string to parse. </param>
             ''' <param name="FieldSource"/> Position of <paramref name="FreeDataText"/> in source line for error highlighting. May be <see langword="null"/>. 
             ''' <remarks>
             ''' <para>
             ''' <paramref name="FreeDataText"/> is required to look <b>like in ipkt, A0, A1</b>: <code>AttName|AttValue|AttName|AttValue|comment</code>. 
             ''' There may be zero, one or more attributes (name/value pairs). The attribute names are trimmed.
             ''' </para>
             ''' <para>
             ''' This method clears and sets the <see cref="GeoPoint.Attributes"/> and <see cref="GeoPoint.Comment"/> properties.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> At least one found attribute has an empty name. </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> At least two found attributes have the same name. </exception>
             ''' <exception cref="Rstyx.Utilities.IO.ParseException"> Any other Exception will be wrapped by a <see cref="Rstyx.Utilities.IO.ParseException"/>. </exception>
            Public Sub ParseFreeData(FreeDataText As String, FieldSource As DataFieldSource)
                Try
                    Me.Attributes.Clear()
                    Me.Comment = String.Empty
                    If (FieldSource Is Nothing) Then FieldSource = New DataFieldSource(0, 0, String.Empty)

                    If (FreeDataText.IsNotEmptyOrWhiteSpace()) Then
                        Dim i            As Integer
                        Dim AttCount     As Integer = 0
                        Dim FreeFields() As String  = FreeDataText.Split(AttSeparator)
                        
                        For i = 0 To FreeFields.Length - 3 Step 2
                            
                            Dim AttName As String = FreeFields(i).Trim()
                            Dim ErrMsg  As String = Nothing

                            AttCount += 1
                            
                            ' Verify Attribute Name.
                            If (AttName.IsEmptyOrWhiteSpace()) Then
                                ErrMsg = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseFreeData_AttName_Empty, AttCount)
                            ElseIf (Me.Attributes.ContainsKey(AttName)) Then
                                ErrMsg = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseFreeData_AttName_Repeated, AttCount, AttName)
                            End If
                            If (ErrMsg IsNot Nothing) Then
                                Dim LineNo As Integer = If(Me.SourceLineNo < 0, 0, Me.SourceLineNo)
                                Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                                        LineNo,
                                                                        FieldSource.Column,
                                                                        FieldSource.Column + FieldSource.Length,
                                                                        ErrMsg,
                                                                        If(LineNo > 0, Me.SourcePath, Nothing)))
                            End If
                            
                            Me.Attributes.Add(AttName, FreeFields(i + 1))
                        Next
                        
                        Me.Comment = FreeFields(i)
                        If (FreeFields.Length > (i + 1)) Then
                            Me.Comment &= AttSeparator & FreeFields(i + 1)
                        End If
                    End If
                    
                Catch ex As Exception When (TypeOf ex IsNot ParseException)
                    Dim LineNo As Integer = If(Me.SourceLineNo < 0, 0, Me.SourceLineNo)
                    Throw New ParseException(New ParseError(ParseErrorLevel.[Error],
                                                            LineNo,
                                                            FieldSource.Column,
                                                            FieldSource.Column + FieldSource.Length,
                                                            Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseFreeData_UnexpectedError,
                                                            If(LineNo > 0, Me.SourcePath, Nothing)
                                                           ), ex)
                End Try
            End Sub
            
        #End Region
            
        #Region "Info Text - Parsing and Creating"
            
            ''' <summary> Creates a point info text for file output, containing iGeo "iTrassen-Codierung" or else cant, kind text and info. </summary>
             ''' <param name="Options"> Controls content of created text. </param>
             ''' <returns> The point info text for file output, i.e. 'u= 23  info'. </returns>
             ''' <remarks>
             ''' <para>
             ''' Depending on <paramref name="Options"/> "iTrassen-Codierung" will be created 
             ''' or else special info will be added to pure info (<see cref="GeoPoint.Info"/>):
             ''' <list type="table">
             ''' <listheader> <term> <b>Option</b> </term>  <description> Result example </description></listheader>
             ''' <item> <term> <see cref="GeoPointOutputOptions.Create_iTC"/>               </term>  <description> 2-v1 # 12/28 </description></item>
             ''' <item> <term> <see cref="GeoPointOutputOptions.CreateInfoWithActualCant"/> </term>  <description> u= 23  info  </description></item>
             ''' <item> <term> <see cref="GeoPointOutputOptions.CreateInfoWithPointKind "/> </term>  <description> GVP  info    </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Overrides Function CreateInfoTextOutput(Options As GeoPointOutputOptions) As String
                
                Dim RetText As String
                
                If (Options.HasFlag(GeoPointOutputOptions.Create_iTC)) Then
                    RetText = Me.Create_iTC()
                Else
                    RetText = MyBase.CreateInfoTextOutput(Options)
                End If
                
                Return RetText.TrimToMaxLength(25)
            End Function
            
            ''' <summary>
            ''' <see cref="GeoPoint.Info"/> (and maybe <see cref="GeoPoint.Comment"/>) will be parsed for some info. 
            ''' The found values will be stored into point properties. 
            ''' </summary>
             ''' <param name="Options">  Controls what target info should be parsed for. </param>
             ''' <remarks>
             ''' <para>
             ''' See <see cref="GeoPoint.ParseInfoTextInput"/> for details.
             ''' </para>
             ''' <para>
             ''' <see cref="GeoIPoint"/> special: Depending on <paramref name="Options"/> iGeo "iTrassen-Codierung" will be parsed.
             ''' </para>
             ''' </remarks>
            Public Overrides Function ParseInfoTextInput(Options As GeoPointEditOptions) As ParseInfoTextResult
                
                Dim RetValue As New ParseInfoTextResult()
                
                ' Parse iGeo "iTrassen-Codierung".
                If (Options.HasFlag(GeoPointEditOptions.Parse_iTC)) Then
                    RetValue = Me.Parse_iTC(TryComment:=Options.HasFlag(GeoPointEditOptions.ParseCommentToo))
                End If
                
                ' Standard kind guessing. This doesn't overrides already recognized values.
                Dim Dummy As ParseInfoTextResult = MyBase.ParseInfoTextInput(Options)
                
                Return RetValue
            End Function
            
            ''' <summary> Creates iGeo "iTrassen-Codierung" for ipkt text field, containing iGeo point kind codes and text info. </summary>
             ''' <returns> The iGeo "iTrassen-Codierung" for ipkt text field. </returns>
            Protected Function Create_iTC() As String
                
                Dim IpktText As String = String.Empty
                
                ' Special mark type (iGeo Trassen-Absteckbuch).
                If (Me.MarkTypeAB.IsNotEmptyOrWhiteSpace()) Then
                    IpktText = Me.MarkTypeAB
                End If
                
                ' Point kind.
                If (Not (Me.Kind = GeoPointKind.None)) Then
                    Select Case Me.Kind
                        Case GeoPointKind.Platform : IpktText &= "-b"

                        Case GeoPointKind.RailTop1 : IpktText &= "-s1"
                        Case GeoPointKind.RailTop2 : IpktText &= "-s2"

                        Case GeoPointKind.MeasurePoint1 : IpktText &= "-m1"
                        Case GeoPointKind.MeasurePoint2 : IpktText &= "-m2"

                        Case GeoPointKind.RailsFixPoint
                            
                            IpktText &= "-v"
                            If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType

                        Case GeoPointKind.FixPoint, GeoPointKind.FixPoint1D, GeoPointKind.FixPoint2D, GeoPointKind.FixPoint3D
                            
                            IpktText &= "-f"
                            If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                            
                        Case GeoPointKind.Rails
                            
                            Dim IsNanActualCantAbs = Double.IsNaN(Me.ActualCantAbs)
                            Dim IsNanActualCant    = Double.IsNaN(Me.ActualCant)
                            Dim IsNanActualGauge   = Double.IsNaN(Me.ActualTrackGauge)

                            If (IsNanActualCant AndAlso IsNanActualCantAbs AndAlso IsNanActualGauge) Then
                                IpktText &= "-i"
                            Else
                                If (Not IsNanActualCantAbs) Then IpktText &= sprintf("-iueb=%-5.1f", Me.ActualCantAbs * 1000 * -1).Replace(".0", String.Empty)
                                If (Not IsNanActualCant)    Then IpktText &= sprintf("-iu=%-5.1f"  , Me.ActualCant    * 1000).Replace(".0", String.Empty)
                            End If

                            If (Not IsNanActualGauge) Then IpktText &= sprintf("-sp=%-6.1f", Me.ActualTrackGauge * 1000).Replace(".0", String.Empty)

                    End Select
                End If
                
                ' Info.
                If (Me.Info.IsNotEmptyOrWhiteSpace()) Then
                    If (IpktText.IsNotEmpty()) Then
                        IpktText &= " # "
                    End If
                    IpktText &= Me.Info
                End If
                
                Return IpktText
            End Function
            
            ''' <summary> <see cref="GeoPoint.Info"/> will be parsed as iGeo "iTrassen-Codierung". </summary>
             ''' <param name="TryComment"> If <see langword="true"/> then <see cref="GeoPoint.Info"/> and <see cref="GeoPoint.Comment"/> will be parsed. </param>
             ''' <remarks>
             ''' <para>
             ''' Point kind codes are required to have the expected format: 
             ''' <list type="table">
             ''' <listheader> <term> <b>Format / Pattern</b> </term>  <description> Point Kind </description></listheader>
             ''' <item> <term> -b                   </term>  <description> Platform                                              </description></item>
             ''' <item> <term> -iueb[=]?[+-]?[0-9]+ </term>  <description> Actual rails with actual absolute cant (sign as iGeo) </description></item>
             ''' <item> <term> -iu[=]?[+-]?[0-9]+   </term>  <description> Actual rails with actual relative cant                </description></item>
             ''' <item> <term> -i                   </term>  <description> Actual rails without actual cant                      </description></item>
             ''' <item> <term> -v[0-9]+             </term>  <description> Rails fix point [with numeric mark type]              </description></item>
             ''' <item> <term> -f[0-9]+             </term>  <description> Other fix point [with numeric mark type]              </description></item>
             ''' <item> <term> ?-                   </term>  <description> If one of the above codes is there, it may be preceeded by a single word character which will be treated as <see cref="GeoIPoint.MarkTypeAB"/>. </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' 
             ''' </para>
             ''' <para>
             ''' This method may set the following properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property</b> </term>  <description> Constraints </description></listheader>
             ''' <item><term> <see cref="GeoPoint.Info"/>           </term>  <description> Set to text part of iTC </description></item>
             ''' <item><term> <see cref="GeoPoint.Kind"/>           </term>  <description> Only, if it isn't set yet </description></item>
             ''' <item><term> <see cref="GeoPoint.MarkType"/>       </term>  <description> Only, if it isn't set yet </description></item>
             ''' <item><term> <see cref="GeoIPoint.ActualCantAbs"/> </term>  <description> Only, if it isn't set yet </description></item>
             ''' <item><term> <see cref="GeoIPoint.ActualCant"/>    </term>  <description> Only, if it isn't set yet </description></item>
             ''' <item><term> <see cref="GeoIPoint.MarkTypeAB"/>    </term>  <description> Only, if it isn't set yet </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Protected Function Parse_iTC(Optional TryComment As Boolean = False) As ParseInfoTextResult
                
                Dim RetValue    As New ParseInfoTextResult()
                Dim j           As Integer = 0
                Dim SearchCount As Integer = If(TryComment, 2, 1)
                Dim SearchText  As String  = Me.Info
                Dim Success     As Boolean = False
                
                Do
                    j += 1
                    
                    If (SearchText.IsNotEmptyOrWhiteSpace()) Then
                        
                        Dim oMatch  As Match = Regex.Match(SearchText, iTC_Pattern)
                        
                        If (oMatch.Success) Then
                            
                            Dim iTC_Kind          As GeoPointKind = GeoPointKind.None
                            Dim iTC_MarkType      As String  = Nothing
                            Dim iTC_ActualCantAbs As Double  = Double.NaN
                            Dim iTC_ActualCant    As Double  = Double.NaN
                            Dim iTC_ActualGauge   As Double  = Double.NaN
                            Dim ApplyNewKind      As Boolean = False
                            Dim IsKindConflict    As Boolean = False
                            
                            ' Recognize point kinds with attached info.
                            For i As Integer = 3 To 111
                                If (oMatch.Groups(i).Success) Then
                                    Dim Key As String = oMatch.Groups(i).Value
                                    Select Case Key
                                        Case "-b":             iTC_Kind = GeoPointKind.Platform
                                        Case "-v":             iTC_Kind = GeoPointKind.RailsFixPoint : iTC_MarkType      = oMatch.Groups(i + 1).Value
                                        Case "-f":             iTC_Kind = GeoPointKind.FixPoint      : iTC_MarkType      = oMatch.Groups(i + 1).Value
                                        Case "-iueb", "-ueb":  iTC_Kind = GeoPointKind.Rails         : iTC_ActualCantAbs = -1 * CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-iu"  , "-u":    iTC_Kind = GeoPointKind.Rails         : iTC_ActualCant    =      CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-sp":            iTC_Kind = GeoPointKind.Rails         : iTC_ActualGauge   =      CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-i":             iTC_Kind = GeoPointKind.Rails
                                        Case "-s1":            iTC_Kind = GeoPointKind.RailTop1
                                        Case "-s2":            iTC_Kind = GeoPointKind.RailTop2
                                        Case "-m1":            iTC_Kind = GeoPointKind.MeasurePoint1
                                        Case "-m2":            iTC_Kind = GeoPointKind.MeasurePoint2
                                    End Select
                                    ' Exit For -> not if rails, because "-iueb", "-iu" and "-sp" may be there.
                                    If ((iTC_Kind <> GeoPointKind.None) AndAlso (iTC_Kind <> GeoPointKind.Rails)) Then
                                        Exit For
                                    End If
                                End If
                            Next
                                            
                            ' Fixpoint: Try to state kind more precisely from mark type.
                            If ((iTC_Kind = GeoPointKind.FixPoint) AndAlso iTC_MarkType.IsNotEmptyOrWhiteSpace()) Then
                                If (MarkType2Kind.ContainsKey(iTC_MarkType)) Then
                                    Dim KindFromMark As GeoPointKind = MarkType2Kind(iTC_MarkType)
                                    Select Case KindFromMark
                                        Case GeoPointKind.FixPoint1D, GeoPointKind.FixPoint2D, GeoPointKind.FixPoint3D :  iTC_Kind = KindFromMark
                                    End Select
                                End If
                            End If
                            
                            ' Check for kind conflict and applying.
                            If (iTC_Kind <> GeoPointKind.None) Then
                                IsKindConflict = ((Me.Kind <> GeoPointKind.None) AndAlso (Me.Kind <> iTC_Kind))
                                ApplyNewKind   = True
                                If (IsKindConflict) Then
                                    If (Me.Kind = GeoPointKind.FixPoint) Then
                                        Select Case iTC_Kind
                                            Case GeoPointKind.FixPoint1D, GeoPointKind.FixPoint2D, GeoPointKind.FixPoint3D
                                                IsKindConflict = False
                                                ApplyNewKind   = True
                                        End Select
                                    ElseIf (iTC_Kind = GeoPointKind.FixPoint) Then    
                                        Select Case Me.Kind
                                            Case GeoPointKind.FixPoint1D, GeoPointKind.FixPoint2D, GeoPointKind.FixPoint3D
                                                IsKindConflict = False
                                                ApplyNewKind   = False
                                        End Select
                                    End If
                                End If
                            End If
                            
                            ' Try to apply recognized point kinds with related info.
                            If (IsKindConflict) Then
                                
                                RetValue.HasConflict = True
                                RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_Kind, Me.ID, Me.Kind.ToDisplayString(), iTC_Kind.ToDisplayString())
                                RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                Select Case Me.Kind
                                    Case GeoPointKind.FixPoint1D, GeoPointKind.FixPoint2D, GeoPointKind.FixPoint3D
                                        RetValue.Hints &= vbNewLine & Me.GetListMarkType2Kind()
                                End Select
                                
                            ElseIf (iTC_Kind <> GeoPointKind.None) Then
                                
                                If (ApplyNewKind) Then
                                    Me.Kind = iTC_Kind
                                End If
                                
                                If (iTC_MarkType.IsNotEmptyOrWhiteSpace()) Then
                                    If (Me.MarkType.IsNotEmptyOrWhiteSpace() AndAlso (Me.MarkType <> iTC_MarkType)) Then
                                        RetValue.HasConflict = True
                                        RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_MarkType, Me.ID, Me.MarkType, iTC_MarkType)
                                        RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                    Else
                                        Me.MarkType = iTC_MarkType
                                    End If
                                End If
                                
                                If (Not Double.IsNaN(iTC_ActualCantAbs)) Then
                                    If ((Not Double.IsNaN(Me.ActualCantAbs)) AndAlso (Not Me.ActualCantAbs.EqualsTolerance(iTC_ActualCantAbs, 0.0006))) Then
                                        RetValue.HasConflict = True
                                        RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_CantAbs, Me.ID, -1 * Me.ActualCantAbs, -1 * iTC_ActualCantAbs)
                                        RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                    Else
                                        Me.ActualCantAbs = iTC_ActualCantAbs
                                    End If
                                End If
                                
                                If (Not Double.IsNaN(iTC_ActualCant)) Then
                                    If ((Not Double.IsNaN(Me.ActualCant)) AndAlso (Not Me.ActualCant.EqualsTolerance(iTC_ActualCant, 0.0006))) Then
                                        RetValue.HasConflict = True
                                        RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_Cant, Me.ID, Me.ActualCant, iTC_ActualCant)
                                        RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                    Else
                                        Me.ActualCant = iTC_ActualCant
                                    End If
                                End If
                                
                                If (Not Double.IsNaN(iTC_ActualGauge)) Then
                                    If ((Not Double.IsNaN(Me.ActualTrackGauge)) AndAlso (Not Me.ActualTrackGauge.EqualsTolerance(iTC_ActualGauge, 0.0006))) Then
                                        RetValue.HasConflict = True
                                        RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_Gauge, Me.ID, Me.ActualTrackGauge, iTC_ActualGauge)
                                        RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                    Else
                                        Me.ActualTrackGauge = iTC_ActualGauge
                                    End If
                                End If
                            End If
                            
                            ' Special mark type.
                            If (oMatch.Groups(1).Success) Then
                                Dim iTC_MarkTypeAB As String = oMatch.Groups(1).Value
                                
                                If ((Me.MarkTypeAB.IsNotEmptyOrWhiteSpace()) AndAlso (Me.MarkTypeAB <> iTC_MarkTypeAB)) Then
                                    RetValue.HasConflict = True
                                    RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_MarkTypeAB, Me.ID, Me.MarkTypeAB, iTC_MarkTypeAB)
                                    RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                Else 
                                    Me.MarkTypeAB = iTC_MarkTypeAB
                                End If
                            End If
                            
                            ' Point info text.
                            If (oMatch.Groups(114).Success) Then
                                Dim iTC_InfoText As String = oMatch.Groups(114).Value.Trim()
                                
                                If ((j = 1) OrElse Me.Info.IsEmptyOrWhiteSpace()) Then
                                    Me.Info = iTC_InfoText
                                Else
                                    Me.Info &= "; " & iTC_InfoText
                                End If
                            Else
                                If (j = 1) Then
                                    Me.Info = String.Empty
                                End If
                            End If
                            
                        End If
                    End If
                    
                    SearchText = Me.Comment
                    
                Loop Until (Success OrElse (j = SearchCount))
                
                Return RetValue
            End Function
            
        #End Region
        
        #Region "Overrides"
            
            ''' <summary> Parses the given string as new <see cref="GeoPoint.ID"/> for this GeoPoint. </summary>
             ''' <param name="TargetID"> The string which is intended to become the new point ID. It will be trimmed here. </param>
             ''' <returns> The parsed ID. </returns>
             ''' <remarks>
             ''' <para>
             ''' This method will be called when setting the <see cref="GeoPoint.ID"/> property.
             ''' It allows for special format conversions and for validation of <paramref name="TargetID"/>.
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> <paramref name="TargetID"/> is longer than 20 characters. </exception>
            Protected Overrides Function ParseID(TargetID As String) As String
                
                Dim RetValue As String = MyBase.ParseID(TargetID)
                
                If (RetValue.Length > MaxIDLength) Then
                    Throw New InvalidIDException(sprintf(Rstyx.Utilities.Resources.Messages.GeoPointConstraints_IDToLong, RetValue, MaxIDLength))
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Sets this point's <see cref="IGeoPoint"/> properties from a given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoIPoint"/>, then all properties will be taken. 
             ''' Otherwise, all <see cref="IGeoPoint"/> interface properties (including <see cref="Attributes"/>) will be assigned 
             ''' to properties of this point, and selected other properties will be converted to attributes.
             ''' </para>
             ''' <para>
             ''' All attributes from <paramref name="SourcePoint"/>, matching properties that don't belong to <see cref="IGeoPoint"/> interface,
             ''' and are declared in <see cref="PropertyAttributes"/>, will be <b>converted to properties</b>, if the properties have no value yet:
             ''' <list type="table">
             ''' <listheader> <term> <b>Attribute Name</b> </term>  <description> <b>Property Name</b> </description></listheader>
             ''' <item> <term> VArtAB   </term>  <description> MarkTypeAB </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Protected Overrides Sub GetPropsFromIGeoPoint(SourcePoint As IGeoPoint)
                
                If (SourcePoint IsNot Nothing) Then
                    
                    MyBase.GetPropsFromIGeoPoint(SourcePoint)
                    
                    If (TypeOf SourcePoint Is GeoIPoint) Then
                        
                        Dim SourceIPoint As GeoIPoint = DirectCast(SourcePoint, GeoIPoint)
                        
                        Me.AttKey1     = SourceIPoint.AttKey1
                        Me.AttKey2     = SourceIPoint.AttKey2
                        Me.AttValue1   = SourceIPoint.AttValue1
                        Me.AttValue2   = SourceIPoint.AttValue2
                        Me.CalcCode    = SourceIPoint.CalcCode
                        Me.CoordType   = SourceIPoint.CoordType
                        Me.Flags       = SourceIPoint.Flags
                        Me.GraficsCode = SourceIPoint.GraficsCode
                        Me.GraficsDim  = SourceIPoint.GraficsDim
                        Me.GraficsEcc  = SourceIPoint.GraficsEcc
                        Me.MarkTypeAB  = SourceIPoint.MarkTypeAB
                    
                        Me.RemovePropertyAttributes()
                        
                    Else
                        Me.CoordType = GeoIPoint.DefaultCoordType
                        Me.ConvertPropertyAttributes()
                    End If
                    
                End If
            End Sub
            
            ''' <summary> Returns a very basic output of the point. </summary>
            Public Overrides Function ToString() As String
                Return sprintf(" %+20s %15.5f%15.5f%15.4f  %s", Me.ID, Me.Y, Me.X, Me.Z, Me.Info)
            End Function
            
        #End Region

    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
