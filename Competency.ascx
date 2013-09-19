<%@ Control Language="VB" AutoEventWireup="false" CodeFile="Competency.ascx.vb" Inherits="performanceeval_content_Competency" %>
 
<div id="PerformanceEvalWrapper">
    <div class="titleBar2">Employee Evaluation <span ID="titleyear" runat="server" /></div>
      	
	<div id="evalContent">
	<asp:hiddenfield ID="hdnSectionIsSaved" runat="server"/>
	
    <h3>Competencies</h3>
    <p>Describe this employee’s performance on the following competencies and behaviors:</p>
    <asp:bulletedlist ID="liCompetencyRating" runat="server" style="list-style-type:none;"/>
    <br/>

    <asp:Repeater ID="RpCompetencyQuestions" runat="server">

        <ItemTemplate>
        <h4><asp:Label ID="lblCompetencyTitle" runat="server" Text='<%#Eval("Title") %>'/></h4>
	    <asp:hiddenfield ID="hdnthisQuestionID" runat="server" Value='<%#Eval("QuestionID") %>'/>
        <p><asp:label ID="lblCompetencyDescription" runat="server" Text='<%#Eval("Description") %>' cssclass="questionlabel_long"/></p>              
        <div>
		   <asp:textbox ID="tbxResponse" runat="server" cssclass="ResponseTextbox_competency" TextMode="MultiLine" 
                        Text='<%#Eval("Response") %>'></asp:textbox>
	    </div>
        <div class="ScoreLabel">Score:</div>
	    <asp:RadioButtonList ID="RatingList" runat="server" cssclass="RatingList" RepeatDirection ="Horizontal" />         
        <div class="clearFix"></div>
        </ItemTemplate>

     </asp:Repeater>
 
            <div id="Buttons" class="evalButtons">
                <asp:Button ID="btnSave" runat="server" Text="Save" />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" />
                <asp:Button ID="btnExit" runat="server" Text="Exit" />
            </div>
 
    </div>
   
</div>
