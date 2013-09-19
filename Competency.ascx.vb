#Region " IMPORTS "
Imports System
Imports System.Data
Imports System.Data.SqlClient
Imports System.Collections.Generic
#End Region

Partial Class performanceeval_content_Competency
    Inherits System.Web.UI.UserControl
    Shared CompetencyList As New List(Of Competency)   '3---create CompetencyList to hold competency objects
    Shared RatingItemList As New List(Of RatingItem)   '3---create RatingItemList to hold raitingitem objects

#Region " GLOBAL VARIABLES "
    '-- user information
    Dim myalias As String   '-- get alias of user

#End Region

    Private ReadOnly Property pgRecordEvalID() As String
        Get
            Return Request.QueryString("re")
        End Get
    End Property

    Private ReadOnly Property PgEvalType() As Integer
        Get
            Return Request.QueryString("type")
        End Get

    End Property

#Region "Classes"

    Public Class Competency

        Dim _QuestionID As Integer
        Public Property QuestionID() As Integer
            Get
                Return _QuestionID
            End Get
            Set(ByVal value As Integer)
                _QuestionID = value
            End Set
        End Property

        Dim _Title As String
        Public Property Title() As String
            Get
                Return _Title
            End Get
            Set(ByVal value As String)
                _Title = value
            End Set
        End Property

        Dim _Description As String
        Public Property Description() As String
            Get
                Return _Description
            End Get
            Set(ByVal value As String)
                _Description = value
            End Set
        End Property

        Dim _Response As String
        Public Property Response() As String
            Get
                Return _Response
            End Get
            Set(ByVal value As String)
                _Response = value
            End Set
        End Property

        Dim _Score As String
        Public Property Score() As String
            Get
                Return _Score
            End Get
            Set(ByVal value As String)
                _Score = value
            End Set
        End Property


        Public Sub New()

        End Sub

    End Class  '1---create competency object 


    Public Class RatingItem
        Dim _ItemName As String
        Public Property ItemName() As String
            Get
                Return _ItemName
            End Get
            Set(ByVal value As String)
                _ItemName = value
            End Set
        End Property

        Dim _ItemValue As Integer
        Public Property ItemValue() As Integer
            Get
                Return _ItemValue
            End Get
            Set(ByVal value As Integer)
                _ItemValue = value
            End Set
        End Property

        Dim _ItemDescription As String
        Public Property ItemDescription() As String
            Get
                Return _ItemDescription
            End Get
            Set(ByVal value As String)
                _ItemDescription = value
            End Set
        End Property
    End Class    '1 ---create RatingItem object 

#End Region



#Region "Page Event"

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not IsPostBack Then
            GetRatingItemDescription()   '4---create and fill each ratingitem object
            GetCompetencyQuestionsAndRetrieveResponses()  '4---create and fill each competency object 
        End If

        hdnSectionIsSaved.Value = False
        btnExit.OnClientClick = String.Format("return CheckIsSaved('{0}');", hdnSectionIsSaved.ClientID)

        End Sub

#End Region


#Region "Event Handlers"

        Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
            SaveUserResponse()
        End Sub

#End Region


#Region "Database Methods"

    Public Sub FillRatingItemList()

        RatingItemList.Clear()
        Using conn As New SqlConnection(ConfigurationManager.ConnectionStrings("ExampleDB").ToString)

            Dim cmd As New SqlCommand("dbo.USP_getRating", conn)  'the stored procedure that gets all the rating info
            cmd.CommandType = CommandType.StoredProcedure

            conn.Open()

            Dim dr As SqlDataReader = cmd.ExecuteReader
            While dr.Read
                Dim ratingItem As New RatingItem
                ratingItem.ItemName = String.Format("{0}-{1}", dr("Score"), dr("Rating"))
                ratingItem.ItemValue = dr("Score")
                ratingItem.ItemDescription = dr("RatingDescription")
                RatingItemList.Add(ratingItem)
            End While

            conn.Close()

        End Using

    End Sub

        Public Sub GetRatingItemDescription()
            FillRatingItemList()

            liCompetencyRating.Items.Clear()
            For Each item As RatingItem In RatingItemList
                Dim li As New ListItem
                li.Text = String.Format("{0}: {1}", item.ItemName, item.ItemDescription)
                li.Value = item.ItemValue
                liCompetencyRating.Items.Add(li)
            Next
        End Sub

        Public Sub GetCompetencyQuestionsAndRetrieveResponses()

            CompetencyList.Clear()

            Dim dTable As New DataTable
            Using conn As New SqlConnection((ConfigurationManager.ConnectionStrings("EmployeeEvaluation_DEV").ToString))

                Dim cmd As New SqlCommand("dbo.USP_getCompetencyQuestionsAndResponses", conn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@RecordEvalId", Me.pgRecordEvalID)
                cmd.Parameters.AddWithValue("@LastUpdated", "2013")

                conn.Open()
                dTable.Load(cmd.ExecuteReader)
                conn.Close()


                For Each dr In dTable.Rows
                    Dim competency As New Competency

                    competency.QuestionID = dr("QuestionID")
                    competency.Title = dr("Title")
                    competency.Description = dr("QuesDescription")

                    If dr("response") Is DBNull.Value Then
                        competency.Response = ""
                    Else
                        competency.Response = dr("response")
                    End If


                    If dr("Score") Is DBNull.Value Then
                        competency.Score = 0
                    Else
                        competency.Score = dr("Score")
                    End If

                    CompetencyList.Add(competency)

                Next

            RpCompetencyQuestions.DataSource = CompetencyList  '5---Pointing the CompetencyQuestion repeater to the CompetencyList
                RpCompetencyQuestions.DataBind()

            End Using

        End Sub

        Public Sub SaveUserResponse()

            For Each item As RepeaterItem In RpCompetencyQuestions.Items
                If item.ItemType = ListItemType.Item Or item.ItemType = ListItemType.AlternatingItem Then
                    Dim tbxResponse As TextBox = DirectCast(item.FindControl("tbxResponse"), TextBox)
                    Dim RatingList As RadioButtonList = DirectCast(item.FindControl("RatingList"), RadioButtonList)
                    Dim hdnthisQuestionID As HiddenField = DirectCast(item.FindControl("hdnthisQuestionID"), HiddenField)

                    For Each competency As Competency In CompetencyList
                        If competency.QuestionID = hdnthisQuestionID.Value Then
                            competency.Response = tbxResponse.Text
                            competency.Score = RatingList.SelectedValue
                        End If
                    Next

                End If
            Next

            Using conn As New SqlConnection((ConfigurationManager.ConnectionStrings("EmployeeEvaluation_DEV").ToString))

                Dim cmd As New SqlCommand("USP_UpdateORSaveUserResponse", conn)
                cmd.CommandType = CommandType.StoredProcedure

                cmd.Parameters.Add(New SqlParameter("@RecordEvalID", SqlDbType.Int))
                cmd.Parameters.Add(New SqlParameter("@EvalTypeID", SqlDbType.Int))
                cmd.Parameters.Add(New SqlParameter("@QuestionID", SqlDbType.Int))
                cmd.Parameters.Add(New SqlParameter("@Response", SqlDbType.VarChar))
                cmd.Parameters.Add(New SqlParameter("@Score", SqlDbType.Int))

                conn.Open()

                For Each competency In CompetencyList
                    cmd.Parameters("@RecordEvalID").Value = Me.pgRecordEvalID
                    cmd.Parameters("@EvalTypeID").Value = Me.PgEvalType
                    cmd.Parameters("@QuestionID").Value = competency.QuestionID

                    If String.IsNullOrEmpty(competency.Response) Then
                        cmd.Parameters("@Response").Value = ""
                    Else
                        cmd.Parameters("@Response").Value = competency.Response
                    End If

                    If String.IsNullOrEmpty(competency.Score) Then
                        cmd.Parameters("@Score").Value = DBNull.Value
                    Else
                        cmd.Parameters("@Score").Value = competency.Score
                    End If

                    cmd.ExecuteNonQuery()

                Next

                conn.Close()

            End Using

            hdnSectionIsSaved.Value = True

    End Sub

#End Region


#Region "Methods"

        Protected Sub RpCompetencyQuestions_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.RepeaterItemEventArgs) Handles RpCompetencyQuestions.ItemDataBound

            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then

                Dim competency As Competency = DirectCast(e.Item.DataItem, Competency)
                Dim RatingList As RadioButtonList = DirectCast(e.Item.FindControl("RatingList"), RadioButtonList)

            RatingList.DataSource = RatingItemList    '5---pointing the datasource of RatingList to RatingItemList
                RatingList.DataTextField = "ItemName"
                RatingList.DataValueField = "ItemValue"

                If competency.Score = 0 Then
                    RatingList.SelectedValue = Nothing
                Else
                RatingList.SelectedValue = competency.Score
                End If

                RatingList.DataBind()

        End If

        End Sub

#End Region

    End Class
