<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core"%>
<%@ taglib uri="http://www.springframework.org/tags/form" prefix="form" %>
<%@ taglib uri="http://www.springframework.org/tags" prefix="spring" %>


<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
<title>Part 4</title>
<spring:url value="/static/style.css" var="url" htmlEscape="true"/>
<style type="text/css">
<!--
@import url("${url}");
-->
</style>
</head>

<body>


<ul id="menu">
    <li><spring:url value="/search" var="url" htmlEscape="true"/><a href="${url}" id="menu_Search">Otsi</a></li>
    <li><spring:url value="/addForm" var="url" htmlEscape="true"/><a href="${url}" id="menu_Add">Lisa</a></li>
</ul>

<br /><br /><br />


	<form:form method="post" action="addForm" modelAttribute="customerForm">
	<table class="formTable" id="formTable">
      <tbody>
        <tr>
          <td>Eesnimi:</td>
          <td><form:input disabled="${customerForm.formDisabled}" path="person.firstName" id="firstNameBox" /></td>
        </tr>
        <tr>
          <td>Perekonnanimi:</td>
          <td><form:input disabled="${customerForm.formDisabled}" path="person.lastName" id="surnameBox" /></td>
        </tr>
        <tr>
          <td>Kood:</td>
          <td><form:input disabled="${customerForm.formDisabled}" path="person.code" id="codeBox" /></td>
        </tr>
        
        <tr>
         <td>Tüüp:</td>
         <td>
			<form:select disabled="${customerForm.formDisabled}" path="person.customerType" items="${customerForm.customerTypes}" id="customerTypeSelect" />
         </td>
        </tr>
        
        <tr>
          <td>Telefonid:</td>
          <td></td>
        </tr>

        <c:forEach items="${customerForm.person.phones}" varStatus="status">
        <tr>
          <td></td>
          <td>
          <br />
          	<form:input id="phones${status.index}.id" path="person.phones[${status.index}].id" type="hidden"/>
            <form:select disabled="${customerForm.formDisabled}" path="person.phones[${status.index}].type" items="${customerForm.phoneTypes}" id="phones${status.index}.type" />
            <form:input id="phones${status.index}.value" disabled="${customerForm.formDisabled}" path="person.phones[${status.index}].value" />
            <c:if test="${!customerForm.formDisabled}">
            	<form:input id="phones${status.index}.deletePressed" path="person.phones[${status.index}].deleteButton" type="submit" value="kustuta" class="linkButton" />
            </c:if>
          </td>
        </tr>	
        </c:forEach>
        
        <!--
        <tr>
          <td></td>
          <td><input id="phones0.id" name="phones[0].id" type="hidden" value=""/><br />
            <select id="phones0.type" name="phones[0].type"><option value="phoneType.fixed" selected="selected">Fixed</option><option value="phoneType.mobile">Mobile</option></select>

            <input id="phones0.value" name="phones[0].value" type="text" value=""/>
            
            <input id="phones0.deletePressed" name="phones[0].deletePressed" value="kustuta" class="linkButton" type="submit" value=""/>
            
          </td>
        </tr> !-->
		<c:if test="${!customerForm.formDisabled}">
	        <tr>
	          <td colspan="2" align="right">
	            
	            <form:input id="addPhoneButton" path="addPhoneButton" type="submit" value="Lisa telefon" />
	            
	          </td>
	        </tr>
        </c:if>
        
        <tr>
          <td colspan="2" align="right"><br/>
          <spring:url value="/search" var="url" htmlEscape="true"/>
          <c:set var="backLink" value='<a href="${url}" id="backLink">Tagasi</a>' />
          ${customerForm.formDisabled ? backLink : '<input type="submit" value="Lisa" id="addButton"/>'}
          </td>
        </tr>
      </tbody>
    </table>
		
  	</form:form>
</body>

</html>