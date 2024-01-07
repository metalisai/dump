CREATE TABLE IF NOT EXISTS `ttammear_boards` (
  `id` INTEGER PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `category` INTEGER NOT NULL,
  `name` varchar(255) NOT NULL,
  `users_only` INTEGER NOT NULL
  ) AUTO_INCREMENT=5 ;

INSERT INTO `ttammear_boards` (`id`, `category`, `name`, `users_only`) VALUES
(1, 1, 'Foorum1', 0),
(2, 1, 'Foorum2', 0),
(3, 2, 'Foorum3', 0),
(4, 3, 'Ainult kasutajatele', 0);

CREATE TABLE IF NOT EXISTS `ttammear_categories` (
  `id` INTEGER PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `users_only` tinyint(1) NOT NULL
) AUTO_INCREMENT=4 ;

INSERT INTO `ttammear_categories` (`id`, `name`, `users_only`) VALUES
(1, 'Kategooria1', 0),
(2, 'Kategooria2', 0),
(3, 'Kategooria3', 1);

CREATE TABLE IF NOT EXISTS `ttammear_members` (
  `id` INTEGER PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `username` varchar(99) DEFAULT NULL,
  `email` varchar(99) DEFAULT NULL,
  `password` text NOT NULL,
  `is_admin` tinyint(1) DEFAULT NULL
) AUTO_INCREMENT=7 ;

INSERT INTO `ttammear_members` (`id`, `username`, `email`, `password`, `is_admin`) VALUES
(5, 'admin', 'admin@admin.com', 'e48e2133b5afb8798339ff1bf29dbbd068dfb556', 1),
(6, 'kasutaja', 'kasutaja@kasutaja.com', 'e48e2133b5afb8798339ff1bf29dbbd068dfb556', 0);

CREATE TABLE IF NOT EXISTS `ttammear_messages` (
  `id` INTEGER PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `author` INTEGER NOT NULL,
  `date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `thread` INTEGER NOT NULL,
  `content` text NOT NULL
) AUTO_INCREMENT=27 ;

INSERT INTO `ttammear_messages` (`id`, `author`, `date`, `thread`, `content`) VALUES
(20, 5, '2014-05-21 00:36:33', 16, 'Teema 1 tekst'),
(21, 5, '2014-05-21 00:36:44', 16, 'Teema 1 postitus 2'),
(22, 5, '2014-05-21 00:37:08', 17, 'Esimese postituse sisu'),
(23, 5, '2014-05-21 00:37:58', 18, 'Teema esimese postituse tekst.'),
(24, 5, '2014-05-21 00:38:41', 19, 'Esimese postituse tekst.'),
(25, 5, '2014-05-21 00:39:00', 20, 'teine teema tekst'),
(26, 5, '2014-05-21 00:39:09', 20, 'Postitus');

CREATE TABLE IF NOT EXISTS `ttammear_threads` (
  `id` INTEGER PRIMARY KEY NOT NULL AUTO_INCREMENT,
  `board` INTEGER NOT NULL,
  `name` varchar(255) NOT NULL,
  `dateCreated` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `creator` INTEGER NOT NULL,
  `id_first_msg` INTEGER NOT NULL,
  `users_only` tinyint(1) NOT NULL
) AUTO_INCREMENT=21 ;

INSERT INTO `ttammear_threads` (`id`, `board`, `name`, `dateCreated`, `creator`, `id_first_msg`, `users_only`) VALUES
(10, 5, 'ok', '2014-05-18 14:12:14', 4, 14, 0),
(16, 1, 'Teema 1', '2014-05-21 00:36:33', 5, 20, 0),
(17, 2, 'Foorum2 Teema1', '2014-05-21 00:37:08', 5, 22, 0),
(18, 3, 'Foorum3 Teema1', '2014-05-21 00:37:58', 5, 23, 0),
(19, 4, 'Ainult kasutajatele mõeldud teema', '2014-05-21 00:38:40', 5, 24, 0),
(20, 1, 'Teine teema', '2014-05-21 00:39:00', 5, 25, 0);