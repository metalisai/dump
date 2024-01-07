package dao;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;

import util.FileUtil;
import util.PropertyLoader;

public class SetupDao {

    private static String DB_URL = new PropertyLoader().getProperty("javax.persistence.jdbc.url");

    public void createSchema() {
        executeSqlFile("schema.sql");
    }
    
    public void testData() {
        executeSqlFile("testdata.sql");
    }

    private void executeUpdate(String sql) {
        try (Connection conn = DriverManager.getConnection(DB_URL);
             Statement stmt = conn.createStatement()) {

            stmt.executeUpdate(sql);
        } catch (SQLException e) {
            throw new RuntimeException(e);
        }
    }
    
    public void executeSqlFile(String filename)
	{
		String statements = FileUtil.readFileFromClasspath(filename);
		for(String statement : statements.split(";"))
		{
			if(statement.matches("\\s*"))
				continue;
			 
			executeUpdate(statement);
		}
	}
}
