package nebulous.conquest.control;

import net.dv8tion.jda.api.entities.MessageChannel;
import net.dv8tion.jda.api.entities.Role;
import net.dv8tion.jda.api.events.message.MessageReceivedEvent;
import net.dv8tion.jda.api.hooks.ListenerAdapter;
import net.dv8tion.jda.api.utils.FileUpload;
import org.jetbrains.annotations.NotNull;

import java.io.File;
import java.io.IOException;
import java.util.List;

public class Listener extends ListenerAdapter {
    @Override
    public void onMessageReceived(@NotNull MessageReceivedEvent event) {
        if (event.getMessage().getContentRaw().equals("!map"))
        {
            List<Role> roles = event.getMember().getRoles();
            for (Role role: roles) {
                if (role.getName().equalsIgnoreCase("Developer")) {
                    try {
                        Main.generateSystemMap();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                    MessageChannel channel = event.getChannel();
                    channel.sendMessage("Conquest Map:")
                            .addFiles(FileUpload.fromData(new File("./conquestmap.png")))
                            .queue();
                }
            }
        }
    }
}
