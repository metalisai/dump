<?php
function login() // sisselogimis/väljalogimis nupu funktsionaalsus
{
	if(is_logged_in()) // kasutaja juba sisse logitud
	{
		$_SESSION = array(); // tühjenda sessiooni massiib
		session_destroy(); // hävita sessioon
		header("Location: $myurl?mode=login"); // näita sisselogimis lehte
	}
	else // kasutaja pole sisse logitud
		include_once("view/login.html");
}

function connectDB($host, $user, $pass, $db) // loob ühenduse andmebaasiga
{
	global $link;
	$link = mysqli_connect($host, $user, $pass, $db);
	mysqli_query($link, "SET CHARACTER SET UTF8") or
		die("Error charseti seadistamisel.");
}


function autendi() // kasutaja autentimine
{
	global $myurl;
	$errors=array(); // errorite massib
	$username="";
	$passwd="";
	
	if (isset($_POST['username']) && $_POST['username']!="") { // kasutajanimi väli on edastatud ja pole tühi
		$username=$_POST['username'];
	} else {
		$errors[]="Kasutajanimi puudu"; // oli tühi
	}
	
	if (isset($_POST['pwd']) && $_POST['pwd']!="") {
		$passwd=$_POST['pwd'];
	} else {
		$errors[]="parool puudu";	
	}	
	
	if (empty($errors))
	{	
		global $link;
		$username = mysql_real_escape_string($username);
		$passwd = mysql_real_escape_string($passwd);
		$query = "SELECT password FROM ttammear_members WHERE username = '$username' AND password = SHA1('$passwd')";
		$result = mysqli_query($link, $query) or die("$query - ".mysqli_error($link));
		$row = mysqli_fetch_assoc($result);
		if ($row){ // sisselogimis andmed ühtivad andmebaasis leituga
			loginSession($username);
			header("Location: $myurl?mode=success");
		} else {
			$errors[]="Vale info, proovi uuesti.";	
			include("view/login.html");
		}	
	} else { // errorid
		include("view/login.html");
	}
}

function loginSession($username)
{
	$_SESSION["username"] = $username;	// sessiooni muutuja väärtustamine
	$userid = getUserID($username);
	$_SESSION["databaseID"] = $userid;	// sessiooni muutuja väärtustamine
	$_SESSION["isadmin"] = getAdminrights($userid);
}

function looKonto() // konto loomise funktsionaalus
{
	global $myurl;
	$errors=array();
	$username="";
	$passwd="";
	$passwd2="";
	$email="";
	
	if (isset($_POST['username']) && $_POST['username']!="") {
	  $username=$_POST['username'];
	} else {
	  $errors[]="Kasutajanimi puudu";
	}
	
	if (isset($_POST['pwd']) && $_POST['pwd']!="") {
	  $passwd=$_POST['pwd'];
	} else {
	  $errors[]="parool puudu";	
	}	
	
	if (isset($_POST['pwd2']) && $_POST['pwd2']!="") {
	  $passwd2=$_POST['pwd2'];
	} else {
	  $errors[]="parooli kordus puudu";	
	}
	
	if (isset($_POST['email']) && $_POST['email']!="") {
	  $email=$_POST['email'];
	} else {
	  $errors[]="e-mail puudu";	
	}
	
	if (empty($errors)){
	  if ($passwd==$passwd2){
		global $link;
		$username = mysql_real_escape_string($username);
		$email = mysql_real_escape_string($email);
		$passwd = mysql_real_escape_string($passwd);
		$query = "INSERT INTO ttammear_members (username,email,password,is_admin) VALUES ('$username','$email',SHA1('$passwd'), 0)";
		$result = mysqli_query($link,$query) or die("$query - ".mysqli_error($link));
		if($result){
			loginSession($username);
			header("Location: $myurl?mode=success");
		}
		else{
			header("Location: $myurl?mode=failure");
		}
	  } else {
		$errors[]="Salasõnad erinevad, proovi uuesti.";	
		include("view/register.html");
	  }	
	} else {
	  include("view/register.html");
	}
}

function is_logged_in() // tagastab tõeväärtuse kas kasutaja on sisse logitd või mitte
{
	if(isset($_SESSION["username"]) && $_SESSION["username"]) 
		return true;
	else
		return false;
}

function is_admin() // tagastab tõeväärtuse kas kasutaja on admini õigustega
{
	if(is_logged_in() && isset($_SESSION["isadmin"]) && $_SESSION["isadmin"] == true)
		return true;
	else
		return false;
}

function makePost($threadid) // loob uue postituse teemasse 
{
	if(!is_numeric($threadid)) // error
		return;
	if (isset($_POST['postText']) && $_POST['postText']!="" && is_logged_in()) {
		$postText = mysql_real_escape_string($_POST['postText']);
		$id = mysql_real_escape_string($_SESSION['databaseID']);
		$threadidSafe = mysql_real_escape_string($threadid);
		global $link;
		$query = "INSERT INTO ttammear_messages (author,thread,content) VALUES ('$id','$threadidSafe','$postText')";
		mysqli_query($link,$query) or die("$query - ".mysqli_error($link));
		header("Location: $myurl?mode=posts&threadid=$threadid");
	} else {
	}
}

function makeThread($forumid) // loob uue teema
{
	if(!is_numeric($forumid)) // error
		return;
	if (isset($_POST['postText']) && $_POST['postText']!="" && isset($_POST['title']) && $_POST['title'] && is_logged_in()) {
		$postText = mysql_real_escape_string($_POST['postText']);
		$title = mysql_real_escape_string($_POST['title']);
		$id = mysql_real_escape_string($_SESSION['databaseID']);
		global $link;
		$query = "INSERT INTO ttammear_threads (board,name,creator) VALUES ('$forumid','$title','$id')"; // loob teema
		mysqli_query($link,$query) or die("$query - ".mysqli_error($link));
		$threadid = mysqli_insert_id($link);
		$query = "INSERT INTO ttammear_messages (author,thread,content) VALUES ('$id','$threadid','$postText')"; // loob esimese teema algataja postituse
		mysqli_query($link,$query) or die("$query - ".mysqli_error($link));
		$firstpostid = mysqli_insert_id($link);
		$query = "UPDATE ttammear_threads SET id_first_msg=$firstpostid WHERE id=$threadid"; // Lisab teemale esimese postituse ID
		mysqli_query($link,$query) or die("$query - ".mysqli_error($link));
		header("Location: $myurl?mode=posts&threadid=$threadid");
	} else {
	}
}

function deleteThread($threadid, $forumid) // kustutab teema ( ainult adminitele)
{
	if(!is_numeric($threadid))
		return;
	if(is_admin())
	{
		global $link;
		$query = "DELETE FROM ttammear_threads WHERE id=$threadid";
		mysqli_query($link,$query);
		$query = "DELETE FROM ttammear_messages WHERE thread=$threadid";
		mysqli_query($link,$query);
		header("Location: $myurl?mode=threads&forumid=$forumid");
	}
}

function getThreads($forumid) // tagastab kõik teemad mingis alamfoorumis
{
	if(!is_numeric($forumid))
		return null;
	global $link;
	$query = "SELECT * FROM ttammear_threads WHERE board=$forumid";
	$result = mysqli_query($link, $query) or 
		die("$query - ".mysqli_error($link));
	$temp = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$temp[$i] = $row;
		$i++;
	}
	return $temp;
}

function getPosts($threadid) // tagastab kõik postitused mingis teemas
{
	if(!is_numeric($threadid))
		return;						// error
	global $link;
	$query = "SELECT * FROM ttammear_messages WHERE thread=$threadid";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	$temp = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$temp[$i] = $row;
		$i++;
	}
	return $temp;
}

function getCategories() // tagastab kõik foorumi kategooriad
{
	global $link;
	$query = "SELECT * FROM ttammear_categories";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	$temp = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$temp[$i] = $row;
		$i++;
	}
	return $temp;
}

function getForums($catid) // tagastab kõik alamfoorumid
{
	if(!is_numeric($catid))
		return;						// error
	global $link;
	$query = "SELECT * FROM ttammear_boards WHERE category=$catid";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	$temp = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$temp[$i] = $row;
		$i++;
	}
	return $temp;
}
function getForum($forumid) // tagastab foorumi id kaudu
{
	if(!is_numeric($forumid))
		return;
	global $link;
	$query = "SELECT * FROM ttammear_boards WHERE id=$forumid";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	return mysqli_fetch_assoc($result);
}
function getCategory($catid) // tagastab kategooria id kaudu
{
	if(!is_numeric($catid))
		return;
	global $link;
	$query = "SELECT * FROM ttammear_categories WHERE id=$catid";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	return mysqli_fetch_assoc($result);
}
function getThread($threadid) // tagastab teema id kaudu
{
	if(!is_numeric($threadid))
		return;
	global $link;
	$query = "SELECT * FROM ttammear_threads WHERE id=$threadid";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	return mysqli_fetch_assoc($result);
}

function getNumPosts($forumid) // loendab postitused mingis alamfoorumis
{
	if(!is_numeric($forumid))
		return;						// error
	global $link;
	$query = "SELECT id FROM ttammear_threads WHERE board=$forumid";
	$result = mysqli_query($link, $query);
	$forumids = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$forumids[$i] = $row;
		$i++;
	}
	
	$posts = 0;
	if(!empty($forumids))
	{
		$idlist = '';
		foreach($forumids as $id)
		{
			$idlist = $idlist.$id['id'].',';
		}
		$idlist = rtrim($idlist, ",");
		$query = "SELECT id FROM ttammear_messages WHERE thread in ($idlist)";
		$result = mysqli_query($link,$query) or
		die("$query - ".mysqli_error($link));
		$posts = mysqli_num_rows($result);
	}
	
	return $posts;
}

function getUsername($userid) // tagastab kasutajanimi id kaudu
{
	if(!is_numeric($userid))
		return null;
	global $link;
	$query = "SELECT username FROM ttammear_members WHERE id=$userid";
	$result = mysqli_query($link, $query) or 
		die("$query - ".mysqli_error($link));
	$row = mysqli_fetch_assoc($result);
	return $row['username'];
}
function getUserID($username) // tagastab id kasutajanime kaudu
{
	global $link;
	$usernameSafe = mysql_real_escape_string($username);
	$query = "SELECT id FROM ttammear_members WHERE username='$usernameSafe'";
	$result = mysqli_query($link, $query) or 
		die("$query - ".mysqli_error($link));
	$row = mysqli_fetch_assoc($result);
	return $row['id'];
}

function getAdminrights($userid) // tagastab admini õigused kasutaja id kaudu
{
	if(!is_numeric($userid))
		return;
	global $link;
	$query = "SELECT is_admin FROM ttammear_members WHERE id='$userid'";
	$result = mysqli_query($link, $query) or 
		die("$query - ".mysqli_error($link));
	$row = mysqli_fetch_assoc($result);
	return $row['is_admin'];
}

function getAllMembers() // tagastab kõik foorumi liikmed
{
	global $link;
	$query = "SELECT * FROM ttammear_members";
	$result = mysqli_query($link, $query) or
		die("$query - ".mysqli_error($link));
	$members = array();
	$i=0;
	while ($row = mysqli_fetch_assoc($result)) {
		$members[$i] = $row;
		$i++;
	}
	mysqli_free_result($result);
	return $members;
}
?>