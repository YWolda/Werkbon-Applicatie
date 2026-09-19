// Firebase-configuratie van jouw project
const firebaseConfig = {
  apiKey: "AIzaSyBANNZVpGPYpZ1iBsCu1f2SoO97gHjFAu0",
  authDomain: "werkbon-applicatie.firebaseapp.com",
  projectId: "werkbon-applicatie",
  storageBucket: "werkbon-applicatie.firebasestorage.app",
  messagingSenderId: "967824994140",
  appId: "1:967824994140:web:ca1defa27692206d331b99",
  measurementId: "G-VS4M1PKSBF"
};

firebase.initializeApp(firebaseConfig);
const auth = firebase.auth();
const db = firebase.firestore();

// ---------- Authenticatie ----------
window.firebaseAuth = {
    login: async function (email, password) {
        try {
            await auth.signInWithEmailAndPassword(email, password);
            return { success: true };
        } catch (error) {
            return { success: false, message: error.message };
        }
    },

    logout: async function () {
        await auth.signOut();
    },

    registerCallback: function (dotnetHelper) {
        auth.onAuthStateChanged(function (user) {
            if (user) {
                dotnetHelper.invokeMethodAsync('OnAuthStateChanged', user.uid, user.email);
            } else {
                dotnetHelper.invokeMethodAsync('OnAuthStateChanged', null, null);
            }
        });
    }
};

// ---------- Firestore (database) ----------
window.firestoreDb = {
    getDoc: async function (collection, docId) {
        const doc = await db.collection(collection).doc(docId).get();
        if (!doc.exists) return null;
        return JSON.stringify(doc.data());
    },

    setDoc: async function (collection, docId, jsonData) {
        const data = JSON.parse(jsonData);
        await db.collection(collection).doc(docId).set(data, { merge: true });
    },

    addDoc: async function (collection, jsonData) {
        const data = JSON.parse(jsonData);
        const ref = await db.collection(collection).add(data);
        return ref.id;
    },

    deleteDoc: async function (collection, docId) {
        await db.collection(collection).doc(docId).delete();
    },

    // whereField/whereValue zijn optioneel - laat leeg (null) om alles op te halen
    getCollection: async function (collection, whereField, whereValue) {
        let query = db.collection(collection);
        if (whereField) {
            query = query.where(whereField, '==', whereValue);
        }
        const snapshot = await query.get();
        const results = [];
        snapshot.forEach(function (doc) {
            const data = doc.data();
            data.id = doc.id;
            results.push(data);
        });
        return JSON.stringify(results);
    }
};
