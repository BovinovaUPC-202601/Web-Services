using Microsoft.EntityFrameworkCore;
using VacApp_Bovinova_Platform.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace VacApp_Bovinova_Platform.AIAssistant.Infrastructure.Persistence.EFC.Configuration;

public static class AIAssistantSchemaInitializer
{
    public static void EnsureAIAssistantTablesCreated(this AppDbContext context)
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `a-i-sessions` (
                `id` int NOT NULL AUTO_INCREMENT,
                `user_id` int NOT NULL,
                `bovine_id` int NULL,
                `session-type` int NOT NULL,
                `created-at` datetime(6) NOT NULL,
                `updated-at` datetime(6) NOT NULL,
                PRIMARY KEY (`id`)
            ) ENGINE=InnoDB;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `general-chat-sessions` (
                `id` int NOT NULL AUTO_INCREMENT,
                `user_id` int NOT NULL,
                `messages-json` text NOT NULL,
                `created-at` datetime(6) NOT NULL,
                `updated-at` datetime(6) NOT NULL,
                PRIMARY KEY (`id`)
            ) ENGINE=InnoDB;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `bovine-chat-sessions` (
                `id` int NOT NULL AUTO_INCREMENT,
                `user_id` int NOT NULL,
                `bovine_id` int NOT NULL,
                `messages-json` text NOT NULL,
                `created-at` datetime(6) NOT NULL,
                `updated-at` datetime(6) NOT NULL,
                PRIMARY KEY (`id`)
            ) ENGINE=InnoDB;
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS `bovine-analyses` (
                `id` int NOT NULL AUTO_INCREMENT,
                `user_id` int NOT NULL,
                `bovine_id` int NOT NULL,
                `score` decimal(18,2) NOT NULL,
                `visible-issues` text NOT NULL,
                `urgency-level` int NOT NULL,
                `recommendation` text NOT NULL,
                `confidence` decimal(18,4) NOT NULL,
                `created-at` datetime(6) NOT NULL,
                PRIMARY KEY (`id`)
            ) ENGINE=InnoDB;
            """);
    }
}
