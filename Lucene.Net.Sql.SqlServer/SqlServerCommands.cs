namespace Lucene.Net.Sql.SqlServer
{
    internal static class SqlServerCommands
    {
        public const string SetupTablesCommand = @"
CREATE TABLE IF NOT EXISTS `{0}_data_blocks` (
  `node` bigint(20) NOT NULL,
  `seq` int unsigned NOT NULL,
  `data` blob,
  PRIMARY KEY  (`node`, `seq`),
  KEY `seq` (`seq`)
) ENGINE=InnoDB DEFAULT CHARSET=binary;

CREATE TABLE IF NOT EXISTS `{0}_nodes` (
  `node` bigint(20) NOT NULL auto_increment,
  `name` varchar(255) NOT NULL,
  `directory` varchar(255) NOT NULL,
  `size` bigint(20) NOT NULL default '0',
  UNIQUE KEY `name` (`name`,`directory`),
  KEY `node` (`node`),
  KEY `directory` (`directory`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `{0}_locks` ( 
  `anchor` varchar(255) NOT NULL,
  `lock_id` varchar(128) NOT NULL, 
  `created` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP, 
  PRIMARY KEY (`anchor`) 
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
";

        public const string PurgeTablesCommand = @"
BEGIN;

DROP TABLE IF EXISTS `{0}_data_blocks`;
DROP TABLE IF EXISTS `{0}_nodes`;
DROP TABLE IF EXISTS `{0}_locks`;

COMMIT;
";

        public const string ListNodesQuery = @"
SELECT `name` FROM `{0}_nodes`
WHERE `directory` = @directory;
";

        public const string CreateIfNotExistsAndGetNodeQuery = @"
INSERT IGNORE INTO `{0}_nodes`
(
    `name`, `directory`
)
VALUES
(
    @name, @directory
);

SELECT
    `node` as `id`,
    `name`,
    `size`
FROM `{0}_nodes`
WHERE `name` = @name AND `directory` = @directory;
";

        public const string RemoveNodeCommand = @"
BEGIN;

SELECT `node` INTO @node FROM `{0}_nodes` WHERE `name` = @name AND `directory` = @directory;

DELETE FROM `{0}_data_blocks` WHERE `node` = @node;
DELETE FROM `{0}_nodes` WHERE `node` = @node;

COMMIT;
";

        public const string AddLockCommand = @"
INSERT IGNORE INTO `{0}_locks` 
( 
    `anchor`, `lock_id`, `created`
) 
VALUES 
(
    @anchor, @lock_id, now()
)  
ON DUPLICATE KEY UPDATE 
    `lock_id` = if(`created` < now() - interval @max_lock_time_in_seconds second, values(`lock_id`), `lock_id`), 
    `created` = if(`lock_id` = values(`lock_id`), values(`created`), `created`);

SELECT `lock_id` FROM `{0}_locks` WHERE `anchor` = @anchor;
";

        public const string LockExistsQuery = @"
SELECT EXISTS(SELECT * FROM `{0}_locks` WHERE `anchor` = @anchor);
";

        public const string DeleteLockCommand = @"
DELETE FROM `{0}_locks` WHERE `anchor` = @anchor;
";

        public const string GetBlockCommand = @"
SELECT `data` FROM `{0}_data_blocks` WHERE `node` = @node_id AND `seq` = @block;
";

        public const string WriteBlockCommand = @"
BEGIN;

INSERT INTO `{0}_data_blocks`
(
    `node`, `seq`, `data`
)
VALUES
(
    @node_id, @block, @data
)
ON DUPLICATE KEY UPDATE
    `data` = @data;

UPDATE `{0}_nodes`
SET `size` = @size
WHERE `node` = @node_id;

COMMIT;
";
    }
}
