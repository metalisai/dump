<?php 
session_start();
$myurl=$_SERVER['PHP_SELF'];

$link = null;

require_once("functions.php");

include_once("view/header.html");

$host = 'localhost';
$user = 'test';
$pwd = '';
$db = 'test';

connectDB($host,$user,$pwd,$db);


$mode="pealeht";

if (isset($_GET['mode']) && $_GET['mode']!="")
	$mode=$_GET['mode'];	


switch($mode)
{
	case "login":
		login();
	break;
	case "register":
		include_once("view/register.html");
	break;
	
	case "users":
		include_once("view/users.html");
	break;
	
	case "threads":
		include_once("view/threads.html");
	break;
	
	case "posts":
		include_once("view/posts.html");
	break;
	
	case "aut":
		autendi();
	break;
	
	case "newacc":
		looKonto();
	break;
	
	case "makepost":
		makePost($_GET['threadid']);
	break;
	
	case "makethread":
		makeThread($_GET['forumid']);
	break;
	
	case "deletethread":
		deleteThread($_GET['threadid'],$_GET['forumid']);
	break;
	
	case "success":
		include("view/pealeht.html");
	break;
		
	default:
		// muul juhul on lehe reziim pealeht
		include("view/pealeht.html");
	break;
}

include_once("view/footer.html");

?>