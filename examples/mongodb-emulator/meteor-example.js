/**
 * Meteor Example: Using LinksPlatform MongoDB Emulator
 *
 * This example demonstrates how to integrate LinksPlatform MongoDB emulator
 * with a Meteor application.
 *
 * Installation:
 * 1. npm install axios
 * 2. Copy linksplatform-mongodb.js to your Meteor imports folder
 * 3. Use the client as shown below
 */

import { MongoClient } from './linksplatform-mongodb';

// Configuration
const LINKSPLATFORM_URL = process.env.LINKSPLATFORM_URL || 'http://localhost:5000';

// Initialize client
let linksClient;
let linksDb;

/**
 * Initialize LinksPlatform MongoDB connection
 */
async function initializeLinksDB() {
    try {
        linksClient = await MongoClient.connect(LINKSPLATFORM_URL);
        linksDb = linksClient.db('meteor-app');
        console.log('✓ Connected to LinksPlatform MongoDB Emulator');
        return linksDb;
    } catch (error) {
        console.error('Failed to connect to LinksPlatform:', error);
        throw error;
    }
}

/**
 * Example Meteor Methods using LinksPlatform
 */
if (Meteor.isServer) {
    Meteor.methods({
        /**
         * Create a new task in LinksPlatform
         */
        async 'tasks.insert'(text) {
            check(text, String);

            const tasks = linksDb.collection('tasks');
            const result = await tasks.insertOne({
                text: text,
                createdAt: new Date().toISOString(),
                userId: this.userId,
                username: Meteor.user().username
            });

            return result.insertedId;
        },

        /**
         * Get all tasks from LinksPlatform
         */
        async 'tasks.getAll'() {
            const tasks = linksDb.collection('tasks');
            return await tasks.find({}, { limit: 50 });
        },

        /**
         * Update a task
         */
        async 'tasks.update'(taskId, updates) {
            check(taskId, String);
            check(updates, Object);

            const tasks = linksDb.collection('tasks');
            const result = await tasks.updateOne(
                { _id: taskId },
                updates
            );

            return result.modifiedCount > 0;
        },

        /**
         * Delete a task
         */
        async 'tasks.remove'(taskId) {
            check(taskId, String);

            const tasks = linksDb.collection('tasks');
            const result = await tasks.deleteOne({ _id: taskId });

            return result.deletedCount > 0;
        },

        /**
         * Get user's tasks
         */
        async 'tasks.getUserTasks'() {
            if (!this.userId) {
                throw new Meteor.Error('not-authorized');
            }

            const tasks = linksDb.collection('tasks');
            return await tasks.find({ userId: this.userId });
        }
    });

    // Initialize on server startup
    Meteor.startup(async () => {
        await initializeLinksDB();
    });
}

/**
 * Example React Component using LinksPlatform data
 */
if (Meteor.isClient) {
    import React, { useState, useEffect } from 'react';

    function TaskList() {
        const [tasks, setTasks] = useState([]);
        const [newTask, setNewTask] = useState('');
        const [loading, setLoading] = useState(false);

        useEffect(() => {
            loadTasks();
        }, []);

        const loadTasks = async () => {
            setLoading(true);
            try {
                const result = await Meteor.callAsync('tasks.getAll');
                setTasks(result);
            } catch (error) {
                console.error('Error loading tasks:', error);
            } finally {
                setLoading(false);
            }
        };

        const handleSubmit = async (e) => {
            e.preventDefault();
            if (!newTask.trim()) return;

            try {
                await Meteor.callAsync('tasks.insert', newTask);
                setNewTask('');
                await loadTasks();
            } catch (error) {
                console.error('Error adding task:', error);
            }
        };

        const handleDelete = async (taskId) => {
            try {
                await Meteor.callAsync('tasks.remove', taskId);
                await loadTasks();
            } catch (error) {
                console.error('Error deleting task:', error);
            }
        };

        return (
            <div className="task-list">
                <h2>Tasks (Stored in LinksPlatform)</h2>

                <form onSubmit={handleSubmit}>
                    <input
                        type="text"
                        value={newTask}
                        onChange={(e) => setNewTask(e.target.value)}
                        placeholder="Enter a new task..."
                    />
                    <button type="submit">Add Task</button>
                </form>

                {loading ? (
                    <p>Loading tasks...</p>
                ) : (
                    <ul>
                        {tasks.map(task => (
                            <li key={task._id}>
                                <span>{task.text}</span>
                                <small>by {task.username}</small>
                                <button onClick={() => handleDelete(task._id)}>Delete</button>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        );
    }

    export default TaskList;
}

/**
 * Alternative: Using LinksPlatform as a reactive data source
 *
 * You can also create a Meteor publication that polls LinksPlatform:
 */
if (Meteor.isServer) {
    Meteor.publish('linksplatform.tasks', async function() {
        const self = this;
        const tasks = linksDb.collection('tasks');

        // Initial data load
        const initialTasks = await tasks.find({});
        initialTasks.forEach(task => {
            self.added('tasks', task._id, task);
        });

        self.ready();

        // Optional: Set up polling for reactive updates
        const interval = Meteor.setInterval(async () => {
            const currentTasks = await tasks.find({});
            // Compare and send updates...
        }, 5000); // Poll every 5 seconds

        self.onStop(() => {
            Meteor.clearInterval(interval);
        });
    });
}

/**
 * Environment Configuration
 *
 * Add to your settings.json:
 * {
 *   "linksplatform": {
 *     "url": "http://localhost:5000"
 *   }
 * }
 */

export { initializeLinksDB, linksDb };
