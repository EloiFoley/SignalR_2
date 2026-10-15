'use client'

import { useState, useEffect } from 'react'
// On doit commencer par ajouter signalr dans les node_modules: npm install @microsoft/signalr
// Ensuite on inclut la librairie
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import { UserEntry, Channel } from '@/lib/models'
import styles from './chat.module.css'

interface ChatComponentProps {
  hubConnection: HubConnection | null;
  onConnected?: () => void;
}

export default function ChatComponent({ hubConnection, onConnected }: ChatComponentProps) {
  const [message, setMessage] = useState('');
  const [messages, setMessages] = useState<string[]>([]);
  const [usersList, setUsersList] = useState<UserEntry[]>([]);
  const [channelsList, setChannelsList] = useState<Channel[]>([]);
  const [newChannelName, setNewChannelName] = useState('');
  const [selectedChannel, setSelectedChannel] = useState<Channel | null>(null);
  const [selectedUser, setSelectedUser] = useState<UserEntry | null>(null);

  // Configuration des listeners SignalR quand hubConnection change
  useEffect(() => {
    if (!hubConnection) return;

    // Écouter la liste des utilisateurs
    hubConnection.on('UsersList', (data: UserEntry[]) => {
      setUsersList(data);
    });

    // Écouter les nouveaux messages
    hubConnection.on('NewMessage', (msg: string) => {
      setMessages(prev => [...prev, msg]);
    });

    // TODO: Écouter le message pour mettre à jour la liste de channels
    hubConnection.on('ChannelsList', (channels: Channel[]) => {
      setChannelsList(channels);
    })
    // TODO: Écouter le message pour quitter un channel (lorsque le channel est effacé)

    // Tous les handlers sont enregistrés : on peut maintenant démarrer la connexion.
    if (hubConnection.state === HubConnectionState.Disconnected) {
      hubConnection
        .start()
        .then(() => {
          console.log("Connecté au Hub");
          onConnected?.();
        })
        .catch(err => console.log('Error while starting connection: ' + err));
    }

    return () => {
      hubConnection.off('UsersList');
      hubConnection.off('NewMessage');
      hubConnection.off('ChannelsList');
    };
  }, [hubConnection, onConnected]);

  function joinChannel(channel: Channel) {
    if (!hubConnection) return;
    const selectedChannelId = selectedChannel ? selectedChannel.id : 0;
    hubConnection.invoke('JoinChannel', selectedChannelId, channel.id);
    setSelectedChannel(channel);
    setMessages([]); // Vider les messages quand on change de canal
  }

  function sendMessage(e: React.FormEvent) {
    e.preventDefault();
    if (!hubConnection || !message.trim()) return;

    const selectedChannelId = selectedChannel ? selectedChannel.id : 0;
    hubConnection.invoke('SendMessage', message, selectedChannelId, selectedUser?.value.userId);
    setMessage('');
  }

  function userClick(user: UserEntry) {
    if (selectedUser?.key === user.key) {
      setSelectedUser(null);
    } else {
      setSelectedUser(user);
    }
  }

  function createChannel(e: React.FormEvent) {
    e.preventDefault();
    // TODO: Ajouter un invoke pour créer un canal
    hubConnection?.invoke('CreateChannel', newChannelName);
    setNewChannelName('');
  }

  function deleteChannel(channel: Channel) {
    // TODO: Ajouter un invoke pour supprimer un canal
  }

  function leaveChannel() {
    if (!hubConnection) return;
    const selectedChannelId = selectedChannel ? selectedChannel.id : 0;
    hubConnection.invoke('JoinChannel', selectedChannelId, 0);
    setSelectedChannel(null);
    setMessages([]);
  }

  return (
    <div className={styles.container}>
      <div className={styles.content}>
        {/* Utilisateurs connectés */}
        <div className={styles.usersPanel}>
          <h2>Utilisateurs connectés</h2>
          <ul className={styles.usersList}>
            {usersList.map((user) => (
              <li key={user.key}>
                <button
                  className={`${styles.userButton} ${
                    selectedUser?.key === user.key ? styles.selected : ''
                  }`}
                  onClick={() => userClick(user)}
                >
                  {user.key}
                </button>
              </li>
            ))}
          </ul>
        </div>

        {/* Canaux */}
        <div className={styles.channelsPanel}>
          <div className={styles.channelsHeader}>
            <h2>Canaux</h2>
            <button
              className={styles.leaveButton}
              disabled={selectedChannel === null}
              onClick={leaveChannel}
            >
              Quitter le canal
            </button>
          </div>

          <form onSubmit={createChannel} className={styles.createChannelForm}>
            <input
              type="text"
              value={newChannelName}
              onChange={(e) => setNewChannelName(e.target.value)}
              placeholder="Nouveau canal..."
            />
            <button type="submit">Créer</button>
          </form>

          <ul className={styles.channelsList}>
            {channelsList.map((channel) => (
              <li key={channel.id} className={styles.channelItem}>
                <button
                  className={`${styles.channelButton} ${
                    selectedChannel?.id === channel.id ? styles.selected : ''
                  }`}
                  onClick={() => joinChannel(channel)}
                >
                  {channel.title}
                </button>
                <button
                  className={styles.deleteButton}
                  onClick={() => deleteChannel(channel)}
                >
                  ✕
                </button>
              </li>
            ))}
          </ul>
        </div>

        {/* Conversation */}
        <div className={styles.conversationPanel}>
          <h2>Conversation</h2>
          <p className={styles.recipient}>
            Envoyer un message à :{' '}
            <b>
              {selectedUser
                ? selectedUser.key
                : selectedChannel
                  ? selectedChannel.title
                  : 'Tous'}
            </b>
          </p>

          <form onSubmit={sendMessage} className={styles.messageForm}>
            <input
              type="text"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Votre message..."
            />
            <button type="submit">Envoyer</button>
          </form>

          <div className={styles.messagesList}>
            {messages.map((msg, index) => (
              <p key={index} className={styles.message}>
                {msg}
              </p>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
