package util;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;

public class FileUtil {

    public static String readFileFromClasspath(String pathOnClasspath) {
        StringBuilder sb = new StringBuilder();
        try (InputStream is = FileUtil.class.getClassLoader().getResourceAsStream(pathOnClasspath);
             BufferedReader r = new BufferedReader(new InputStreamReader(is, StandardCharsets.UTF_8))) {

            String str = null;
            while ((str = r.readLine()) != null) {
                sb.append(str);
                sb.append("\n");
            }
        } catch (IOException e) {
            throw new RuntimeException(e);
        }

        return sb.toString();
    }

}
