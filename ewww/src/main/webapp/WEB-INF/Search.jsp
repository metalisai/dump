<%@ page language="java" contentType="text/html; charset=UTF-8"
    pageEncoding="UTF-8"%>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core"%>


<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
<title>Part 4</title>
<style type="text/css">
<!--
@import url("static/style.css");
-->
</style>
</head>
<body>


<ul id="menu">
    <li><a href="search" id="menu_Search">Otsi</a></li>
    <li><a href="addForm" id="menu_Add">Lisa</a></li>
    <li><a href="admin?do=clear_data" id="menu_ClearData">Tühjenda</a></li>
    <li><a href="admin?do=insert_data" id="menu_InsertData">Sisesta näidisandmed</a></li>
</ul>

<br /><br /><br />



<form method="get" action="search">
  <input name="searchString" id="searchStringBox" value=""/>
  <input type="submit" id="filterButton" value="Filtreeri" />
<br /><br />
<table class="listTable" id="listTable">
    <thead>
      <tr>
          <th scope="col">Nimi</th>
          <th scope="col">Perekonnanimi</th>
          <th scope="col">Kood</th>
          <th scope="col"></th>
        </tr>
    </thead>
    <tbody>
		<c:forEach var="person" items="${requestScope['people']}" varStatus="status">
		<tr>
          <td>
            <div id="row_${person.code}"><a href="view/${person.code}" id="view_${person.code}">${person.firstName}</a></div>
          </td>
          <td>
            ${person.lastName}
          </td>
          <td>
            ${person.code}
          </td>
          <td>
            <a href="delete/${person.code}" id="delete_${person.code}">Kustuta</a>
          </td>
        </tr>
		</c:forEach>
    </tbody>
</table>
</form>
</body>

</html>