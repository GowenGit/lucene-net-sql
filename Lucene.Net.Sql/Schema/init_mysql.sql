CREATE TABLE IF NOT EXISTS `{0}_data_blocks` (
  `inode` bigint(20) NOT NULL,
  `seq` int unsigned not null,
  `data` blob ,
  PRIMARY KEY  (`inode`, `seq`)
) ENGINE=InnoDB DEFAULT CHARSET=binary;

CREATE TABLE IF NOT EXISTS `{0}_inodes` (
  `inode` bigint(20) NOT NULL auto_increment,
  `name` varchar(255) NOT NULL,
  `directory` varchar(255) NOT NULL,
  `size` bigint(20) NOT NULL default '0',
  UNIQUE KEY `name` (`name`,`directory`),
  KEY `inode` (`inode`)
  KEY `directory` (`directory`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `{0}_locks` ( 
  `anchor` varchar(255) NOT NULL,
  `lock_id` varchar(128) NOT NULL, 
  `created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, 
  PRIMARY KEY (`anchor`) 
) ENGINE=InnoDB DEFAULT CHARSET=utf8;