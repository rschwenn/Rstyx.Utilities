
Imports System.Collections.Generic
Imports System.Linq
Imports System.Math
Imports System.Text.RegularExpressions

Imports Rstyx.Utilities.StringUtils
Imports Rstyx.Utilities.Math

Namespace Domain
    
    ''' <summary> One single iPoint. </summary>
    Public Class GeoIPoint
        Inherits GeoPoint
        
        #Region "Private Fields"
            
            Private Shared ReadOnly MaxIDLength     As Integer = 20
            Private Shared ReadOnly AttSeparator    As Char    = "|"
            
        #End Region
        
        #Region "Constuctor"
            
            ''' <summary> Creates a new, empty <see cref="GeoIPoint"/>. </summary>
            Public Sub New()
            End Sub
            
            ''' <summary> Creates a new <see cref="GeoIPoint"/> and inititializes it's properties from any given <see cref="IGeoPoint"/>. </summary>
             ''' <param name="SourcePoint"> The source point to get init values from. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoIPoint"/>, then all properties will be taken.
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoVEPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> TrackPos.TrackNo   </term>  <description> StrNr </description></item>
             ''' <item> <term> TrackPos.RailsCode </term>  <description> StrRi </description></item>
             ''' <item> <term> TrackPos.Kilometer </term>  <description> StrKm </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' If <paramref name="SourcePoint"/> is a <see cref="GeoTCPoint"/>, then the following properties will be converted to attributes:
             ''' <list type="table">
             ''' <listheader> <term> <b>Property Name</b> </term>  <description> <b>Attribute Name</b> </description></listheader>
             ''' <item> <term> Km </term>  <description> StrKm </description></item>
             ''' </list>
             ''' </para>
             ''' <para>
             ''' The following attributes will be converted to properties:
             ''' <list type="table">
             ''' <listheader> <term> <b>Attribute Name</b> </term>  <description> <b>Property Name</b> </description></listheader>
             ''' <item> <term> VArtAB   </term>  <description> MarkTypeAB </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
             ''' <exception cref="InvalidIDException"> ID of <paramref name="SourcePoint"/> isn't a valid ID for this point. </exception>
            Public Sub New(SourcePoint As IGeoPoint)
                MyBase.New(SourcePoint)
                
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
                    
                Else
                    Me.CoordType = "YXZ"
                    
                    Dim PropertyName   As String
                    Dim AttributeName  As String
                    Dim AttStringValue As String
                    
                    ' Source is VermEsn: Convert VE-unique properties to attributes.
                    If (TypeOf SourcePoint Is GeoVEPoint) Then
                            
                        Dim SourceVEPoint As GeoVEPoint = DirectCast(SourcePoint, GeoVEPoint)
                        
                        PropertyName = "TrackPos.TrackNo"
                        If (SourceVEPoint.TrackPos.TrackNo IsNot Nothing) Then
                            AttributeName = AttributeNames(PropertyName) 
                            If (Not Attributes.ContainsKey(AttributeName)) Then
                                Attributes.Add(AttributeName, sprintf("%4s", SourceVEPoint.TrackPos.TrackNo))
                            End If
                        End If
                        
                        PropertyName = "TrackPos.RailsCode"
                        If (SourceVEPoint.TrackPos.RailsCode.IsNotEmptyOrWhiteSpace()) Then 
                            AttributeName = AttributeNames(PropertyName) 
                            If (Not Attributes.ContainsKey(AttributeName)) Then
                                Attributes.Add(AttributeName, SourceVEPoint.TrackPos.RailsCode)
                            End If
                        End If
                        
                        PropertyName = "TrackPos.Kilometer"
                        If (SourceVEPoint.TrackPos.Kilometer.HasValue()) Then 
                            AttributeName = AttributeNames(PropertyName) 
                            If (Not Attributes.ContainsKey(AttributeName)) Then
                                Attributes.Add(AttributeName, sprintf("%11.4f", SourceVEPoint.TrackPos.Kilometer.Value))
                            End If
                        End If
                    End If
                    
                    
                    ' Source is TC: Convert TC-unique properties to attributes.
                    If (TypeOf SourcePoint Is GeoTcPoint) Then
                            
                        Dim SourceTCPoint As GeoTcPoint = DirectCast(SourcePoint, GeoTcPoint)
                        
                        PropertyName = "Km"
                        If (SourceTCPoint.Km.HasValue()) Then 
                            AttributeName = AttributeNames(PropertyName) 
                            If (Not Attributes.ContainsKey(AttributeName)) Then
                                Attributes.Add(AttributeName, sprintf("%11.4f", SourceTCPoint.Km.Value))
                            End If
                        End If
                    End If
                    
                    
                    ' Convert selected attributes to properties.
                    PropertyName = "MarkTypeAB"
                    If (Me.MarkTypeAB.IsEmpty()) Then
                        AttStringValue = GetAttValueByPropertyName(PropertyName)
                        If (AttStringValue IsNot Nothing) Then
                            Char.TryParse(AttStringValue.Trim(), Me.MarkTypeAB)
                            Attributes.Remove(AttributeNames(PropertyName))
                        End If
                    End If
                    
                End If
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
            Public Property MarkTypeAB  As Char = vbNullChar
            
        #End Region
        
        #Region "Free Data Text - Parsing and Creating"
            
            ''' <summary> Creates a FreeData text for ipkt file, containing attributes and comment. </summary>
             ''' <returns> The FreeData text for ipkt file. </returns>
            Public Function CreateFreeDataText() As String
                
                Dim FreeDataText As String = String.Empty
                                
                ' Attributes.
                If (Me.Attributes?.Count > 0) Then
                    Dim AttString   As String = String.Empty
                    
                    For Each kvp As KeyValuePair(Of String, String) In Me.Attributes.OrderBy(Of String)(Function(ByVal kvp2) kvp2.Key)
                        AttString &= " " & kvp.Key & AttSeparator & kvp.Value & AttSeparator
                    Next
                    FreeDataText &= AttString
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
            Public Sub ParseFreeData(FreeDataText As String)
                
                Me.Attributes = Nothing
                Me.Comment    = String.Empty
                
                If (FreeDataText.IsNotEmptyOrWhiteSpace()) Then
                    
                    Dim FreeFields() As String = FreeDataText.Split(AttSeparator)
                    Dim i As Integer
                    For i = 0 To FreeFields.Length - 3 Step 2
                        If (i = 0) Then Me.Attributes = New Dictionary(Of String, String)
                        Me.Attributes.Add(FreeFields(i).Trim(), FreeFields(i + 1))
                    Next
                    Me.Comment = FreeFields(i)
                    If (FreeFields.Length > (i + 1)) Then
                        Me.Comment &= AttSeparator & FreeFields(i + 1)
                    End If
                End If
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
                
                ' Standard kind guessing.
                If (Me.Kind = GeoPointKind.None) Then
                   RetValue = MyBase.ParseInfoTextInput(Options)
                End If
                
                Return RetValue
            End Function
            
            ''' <summary> Creates iGeo "iTrassen-Codierung" for ipkt text field, containing iGeo point kind codes and text info. </summary>
             ''' <returns> The iGeo "iTrassen-Codierung" for ipkt text field. </returns>
            Protected Function Create_iTC() As String
                
                Dim IpktText As String = String.Empty
                
                ' Special mark type (iGeo Trassen-Absteckbuch).
                If (Me.MarkTypeAB.IsNotEmpty()) Then
                    IpktText = Me.MarkTypeAB
                End If
                
                ' Point kind.
                If (Not (Me.Kind = GeoPointKind.None)) Then
                    Select Case Me.Kind
                        Case GeoPointKind.Platform      : IpktText &= "-b"
                        Case GeoPointKind.RailsFixPoint : IpktText &= "-v" : If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                        Case GeoPointKind.FixPoint      : IpktText &= "-f" : If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                        Case GeoPointKind.FixPoint1D    : IpktText &= "-f" : If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                        Case GeoPointKind.FixPoint2D    : IpktText &= "-f" : If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                        Case GeoPointKind.FixPoint3D    : IpktText &= "-f" : If (Me.MarkType.IsNotEmptyOrWhiteSpace()) Then IpktText &= Me.MarkType
                        Case GeoPointKind.Rails         : 
                            Dim IsNanActualCantAbs = Double.IsNaN(Me.ActualCantAbs)
                            Dim IsNanActualCant    = Double.IsNaN(Me.ActualCant)
                            If (IsNanActualCant AndAlso IsNanActualCantAbs) Then
                                IpktText &= "-i"
                            Else
                                If (Not IsNanActualCantAbs) Then IpktText &= sprintf("-iueb=%-3.0f", Me.ActualCantAbs * 1000 * -1)
                                If (Not IsNanActualCant)    Then IpktText &= sprintf("-iu=%-3.0f", Me.ActualCant * 1000)
                            End If
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
                
                        ' Both "-iueb" and "-iu" may be there (in any order).
                        'Dim Pattern As String = "^\s*(\w)?((-b)|(-v)([0-9]+)?|(-f)([0-9]+)?|(-iueb) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+)|(-i))?\s*((#|x)\s?(.+))?$"
                        'Dim Pattern As String = "^ *(\w)?((-b)|(-v)([0-9]+)?|(-f)([0-9]+)?|(-iueb) *=? *([+-]? *[0-9]+) *(-iu) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+) *(-iueb) *=? *([+-]? *[0-9]+)|(-iueb) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+)|(-i))? *((#|x) ?(.*))?$"
                        Dim Pattern As String = "^ *(\w)?((-b)|(-v)([0-9]+)?|(-f)([0-9]+)?|(-iueb) *=? *([+-]? *[0-9]+) *(-iu) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+) *(-iueb) *=? *([+-]? *[0-9]+)|(-iueb) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+)|(-i))? *((#) ?(.*))?$"
                                    
                        Dim oMatch  As Match  = Regex.Match(SearchText, Pattern)
                        
                        If (oMatch.Success) Then
                            
                            Dim iTC_Kind          As GeoPointKind = GeoPointKind.None
                            Dim iTC_MarkType      As String = Nothing
                            Dim iTC_ActualCantAbs As Double = Double.NaN
                            Dim iTC_ActualCant    As Double = Double.NaN
                            
                            ' Recognize point kindes with related info.
                            For i As Integer = 3 To 20
                                If (oMatch.Groups(i).Success) Then
                                    Dim Key As String = oMatch.Groups(i).Value
                                    Select Case Key
                                        Case "-b":     iTC_Kind = GeoPointKind.Platform
                                        Case "-v":     iTC_Kind = GeoPointKind.RailsFixPoint : iTC_MarkType      = oMatch.Groups(i + 1).Value
                                        Case "-f":     iTC_Kind = GeoPointKind.FixPoint      : iTC_MarkType      = oMatch.Groups(i + 1).Value
                                        Case "-iueb":  iTC_Kind = GeoPointKind.Rails         : iTC_ActualCantAbs = -1 * CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-iu":    iTC_Kind = GeoPointKind.Rails         : iTC_ActualCant    =      CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-i":     iTC_Kind = GeoPointKind.Rails
                                    End Select
                                    'Exit For -> not exit, because both "-iueb" and "-iu" may be there.
                                End If
                            Next
                            
                            ' Try to apply recognized point kindes with related info.
                            If (iTC_Kind <> GeoPointKind.None) Then
                                If ( (Me.Kind <> GeoPointKind.None) AndAlso (Me.Kind <> iTC_Kind)) Then
                                    RetValue.HasConflict = True
                                    RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_Kind, Me.ID, Me.Kind.ToDisplayString(), iTC_Kind.ToDisplayString())
                                    RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                Else
                                    Me.Kind = iTC_Kind
                                    
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
                                End If
                            End If
                            
                            ' Special mark type.
                            If (oMatch.Groups(1).Success) Then
                                Dim iTC_MarkTypeAB As Char = oMatch.Groups(1).Value
                                
                                If ((Me.MarkTypeAB <> vbNullChar) AndAlso (Me.MarkTypeAB <> iTC_MarkTypeAB)) Then
                                    RetValue.HasConflict = True
                                    RetValue.Message     = sprintf(Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_MarkTypeAB, Me.ID, Me.MarkTypeAB, iTC_MarkTypeAB)
                                    RetValue.Hints       = Rstyx.Utilities.Resources.Messages.GeoIPoint_ParseITC_Conflict_RejectITC
                                Else 
                                    Me.MarkTypeAB = iTC_MarkTypeAB
                                End If
                            End If
                            
                            ' Point info text.
                            If (oMatch.Groups(23).Success) Then
                                Dim iTC_InfoText As String = oMatch.Groups(23).Value.Trim()
                                
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
            
            ''' <summary> A given string will be parsed as iGeo "iTrassen-Codierung". </summary>
             ''' <param name="PointText"> The string to parse. May be <see langword="null"/>. </param>
             ''' <remarks>
             ''' <para>
             ''' Point kind codes in <paramref name="PointText"/> are required to have the expected format: 
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
             ''' If "iTrassen-Codierung" hasn't been recognized, then whole <paramref name="PointText"/> will be stored in <see cref="GeoPoint.Info"/>.
             ''' </para>
             ''' <para>
             ''' This method clears and sets the following properties:
             ''' <list type="bullet">
             ''' <item><description> <see cref="GeoPoint.Info"/>           </description></item>
             ''' <item><description> <see cref="GeoPoint.Kind"/>           </description></item>
             ''' <item><description> <see cref="GeoPoint.MarkType"/>       </description></item>
             ''' <item><description> <see cref="GeoIPoint.ActualCantAbs"/> </description></item>
             ''' <item><description> <see cref="GeoIPoint.ActualCant"/>    </description></item>
             ''' <item><description> <see cref="GeoIPoint.MarkTypeAB"/>    </description></item>
             ''' </list>
             ''' </para>
             ''' </remarks>
            Public Sub OLD_Parse_iTC(PointText As String)
                
                Me.Kind          = GeoPointKind.None
                Me.ActualCantAbs = Double.NaN
                Me.ActualCant    = Double.NaN
                Me.MarkType      = String.Empty
                Me.MarkTypeAB    = vbNullChar
                Me.Info          = String.Empty
                
                If (PointText.IsNotEmptyOrWhiteSpace()) Then
                    
                    ' Find delimiter between code and info text.
                    Dim DelimIndex As Integer
                    Dim CodePart   As String
                    Dim InfoPart   As String  = Nothing
                    Dim FirstHash  As Integer = PointText.IndexOf("#"c)
                    
                    ' TODO: DEPRECATED -  Remove support for "x" at the end of 2021!
                    Dim FirstX     As Integer = PointText.IndexOf("x", System.StringComparison.OrdinalIgnoreCase)
                    
                    If ((FirstHash > 0) AndAlso (FirstX > 0)) Then
                        DelimIndex = Min(FirstHash, FirstX)
                    Else
                        DelimIndex = Max(FirstHash, FirstX)
                    End If
                    
                    ' Determine code and info parts of input text.
                    If (DelimIndex > 0) Then
                        CodePart = PointText.Substring(0, DelimIndex).Trim()
                        If (PointText.Length > (DelimIndex + 1)) Then
                            InfoPart = PointText.Substring(DelimIndex + 1)
                            If (InfoPart.IsNotEmptyOrWhiteSpace() AndAlso InfoPart.StartsWith(" ")) Then
                                InfoPart = InfoPart.Substring(1)
                            End If
                        End If
                    Else
                        ' Decide later ...
                        CodePart = PointText
                        InfoPart = PointText
                    End If
                    
                    ' Code part: Find point kind.
                    If (CodePart.IsNotEmptyOrWhiteSpace()) Then
                                    
                        'Dim Pattern As String = "^\s*(\w)?((-b)|(-v)([0-9]+)?|(-f)([0-9]+)?|(-iu) *=? *([+-]? *[0-9]+)|(-i))?"
                        Dim Pattern As String = "^\s*(\w)?((-b)|(-v)([0-9]+)?|(-f)([0-9]+)?|(-iueb) *=? *([+-]? *[0-9]+)|(-iu) *=? *([+-]? *[0-9]+)|(-i))?$"
                        Dim oMatch  As Match  = Regex.Match(CodePart, Pattern)
                        
                        If (oMatch.Success) Then
                            For i As Integer = 3 To 12
                                If (oMatch.Groups(i).Success) Then
                                    Dim Key As String = oMatch.Groups(i).Value
                                    Select Case Key
                                        Case "-b":     Me.Kind = GeoPointKind.Platform
                                        Case "-v":     Me.Kind = GeoPointKind.RailsFixPoint  : Me.MarkType = oMatch.Groups(i + 1).Value
                                        Case "-f":     Me.Kind = GeoPointKind.FixPoint       : Me.MarkType = oMatch.Groups(i + 1).Value
                                        Case "-iueb":  Me.Kind = GeoPointKind.Rails          : Me.ActualCantAbs = -1 * CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-iu":    Me.Kind = GeoPointKind.Rails          : Me.ActualCant = CDbl(oMatch.Groups(i + 1).Value.Replace(" ", String.Empty)) / 1000
                                        Case "-i":     Me.Kind = GeoPointKind.Rails
                                    End Select
                                    Exit For
                                End If
                            Next
                            If (oMatch.Groups(1).Success) Then
                                If ((DelimIndex > 0) OrElse (Me.Kind <> GeoPointKind.None)) Then
                                    Me.MarkTypeAB = oMatch.Groups(1).Value
                                End If
                            End If
                        Else
                            ' There seemed to be a code part but w/o supported code or with invalid syntax.
                            If (DelimIndex > 0) Then
                                ' Reset info part to full text.
                                InfoPart = PointText
                            End If
                        End If
                    End If
                    
                    ' Info part: store.
                    If (InfoPart.IsNotEmptyOrWhiteSpace()) Then
                        If ((DelimIndex > 0) OrElse (Me.Kind = GeoPointKind.None)) Then
                            ' Input text is splitted or else isn't splitted and doesn't contain codes.
                            Me.Info = InfoPart
                        End If
                    End If
                End If
            End Sub
            
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
            
            ''' <summary> Returns a very basic output of the point. </summary>
            Public Overrides Function ToString() As String
                Return sprintf(" %+20s %15.5f%15.5f%15.4f  %s", Me.ID, Me.Y, Me.X, Me.Z, Me.Info)
            End Function
            
        #End Region

    End Class

End Namespace

' for jEdit:  :collapseFolds=2::tabSize=4::indentSize=4:
