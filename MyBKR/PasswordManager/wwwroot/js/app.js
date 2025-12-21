// Генератор пароля
function generatePassword(length = 12, useLower = true, useUpper = true, useNumbers = true, useSymbols = true) {
    const lower = 'abcdefghijklmnopqrstuvwxyz';
    const upper = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const numbers = '0123456789';
    const symbols = '!@#$%^&*()_+[]{}|;:,.<>?';

    let chars = '';
    if (useLower) chars += lower;
    if (useUpper) chars += upper;
    if (useNumbers) chars += numbers;
    if (useSymbols) chars += symbols;

    if (chars === '') return '';

    let password = '';
    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * chars.length);
        password += chars[randomIndex];
    }

    return password;
}
//Расшифровка
async function decrypt(cipherTextBase64, password) {
    const cipherText = Uint8Array.from(atob(cipherTextBase64), c => c.charCodeAt(0));

    // Проверяем "Salted__"
    const saltedHeader = 'Salted__';
    for (let i = 0; i < saltedHeader.length; i++) {
        if (cipherText[i] !== saltedHeader.charCodeAt(i)) {
            throw new Error('Invalid header');
        }
    }

    // Соль — следующие 8 байт
    const salt = cipherText.slice(8, 16);
    const encryptedData = cipherText.slice(16);

    // Генерируем ключ и IV (32 + 16 = 48 байт) через evpKdf
    const keyAndIv = await evpKdf(password, salt, 32, 16); // keySize=32, ivSize=16
    const aesKey = keyAndIv.slice(0, 32);
    const iv = keyAndIv.slice(32, 48);

    // Расшифровка
    const decrypted = await aesCbcDecrypt(aesKey, iv, encryptedData);
    return new TextDecoder().decode(decrypted);
}


// Совместимый с C# Rfc2898DeriveBytes (OpenSSL style)
async function evpKdf(password, salt, keySize, ivSize) {
    const hasher = crypto.subtle;
    const encoder = new TextEncoder();
    const blockSize = 32; // SHA-256
    const derivedBytes = new Uint8Array((keySize + ivSize));
    let derivedKey = new Uint8Array(0);
    let block;

    while (derivedKey.length < (keySize + ivSize)) {
        const data = new Uint8Array(derivedKey.length + password.length + salt.length);
        data.set(derivedKey);
        data.set(encoder.encode(password), derivedKey.length);
        data.set(salt, derivedKey.length + password.length);

        block = new Uint8Array(await hasher.digest('SHA-256', data));
        const copyLength = Math.min(blockSize, (keySize + ivSize) - derivedKey.length);
        const temp = new Uint8Array(derivedKey.length + copyLength);
        temp.set(derivedKey);
        temp.set(block.slice(0, copyLength), derivedKey.length);
        derivedKey = temp;
    }

    return derivedKey;
}

// AES-CBC расшифровка
async function aesCbcDecrypt(key, iv, data) {
    const cryptoKey = await crypto.subtle.importKey(
        'raw',
        key,
        { name: 'AES-CBC' },
        false,
        ['decrypt']
    );

    const decrypted = await crypto.subtle.decrypt(
        { name: 'AES-CBC', iv: iv },
        cryptoKey,
        data
    );

    return new Uint8Array(decrypted);
}

const authHeaders = {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
};

// Элементы DOM
const logoutBtn = document.getElementById('logoutBtn');
const foldersList = document.getElementById('foldersList');
const passwordsList = document.getElementById('passwordsList');
const addPasswordBtn = document.getElementById('addPasswordBtn');
const passwordModal = document.getElementById('passwordModal');
const addPasswordForm = document.getElementById('addPasswordForm');
const cancelPasswordBtn = document.getElementById('cancelPasswordBtn');
const savePasswordBtn = document.getElementById('savePasswordBtn');
// Мастер-пароль (временно храним в памяти)
let masterPassword = '';

// --- Форма добавления пароля ---
addPasswordBtn.addEventListener('click', async () => {
    // Запрос мастер-пароля
    const entered = prompt('Введите мастер-пароль:');
    if (!entered) return;
    masterPassword = entered;

    // Очистка формы
    addPasswordForm.reset();
    document.getElementById('passwordFolderId').innerHTML = '<option value="">Без папки</option>';

    // Загрузка папок
    try {
        const res = await fetch('/api/folder', { headers: authHeaders });
        if (res.ok) {
            const folders = await res.json();
            const select = document.getElementById('passwordFolderId');
            folders.forEach(f => {
                const opt = document.createElement('option');
                opt.value = f.id;
                opt.textContent = f.name;
                select.appendChild(opt);
            });
        }
    } catch (err) {
        console.error('Ошибка загрузки папок:', err);
    }

    // Настройка на "Создать"
    passwordModal.querySelector('h3').textContent = 'Добавить пароль';
    savePasswordBtn.textContent = 'Создать';
    savePasswordBtn.onclick = null;

    // Обработчик для создания
    savePasswordBtn.onclick = async () => {
        const data = {
            title: document.getElementById('passwordTitle').value,
            username: document.getElementById('passwordUsername').value,
            password: document.getElementById('passwordValue').value,
            masterPassword: masterPassword,
            url: document.getElementById('passwordUrl').value || null,
            notes: document.getElementById('passwordNotes').value || null,
            folderId: document.getElementById('passwordFolderId').value || null
        };

        const res = await fetch('/api/password', {
            method: 'POST',
            headers: authHeaders,
            body: JSON.stringify(data)
        });

        if (res.ok) {
           passwordModal.classList.remove('show');
            loadPasswords(null);
        } else {
            alert('Ошибка при создании');
        }

        // Сброс
        savePasswordBtn.onclick = null;
    };

    passwordModal.classList.add('show');
});

cancelPasswordBtn.addEventListener('click', () => {
    passwordModal.classList.remove('show');
});
const generatePasswordBtn = document.getElementById('generatePasswordBtn');

generatePasswordBtn.addEventListener('click', () => {
    const passwordInput = document.getElementById('passwordValue');

    // Настройки (можно улучшить через UI позже)
    const length = 16;
    const password = generatePassword(length, true, true, true, true);

    passwordInput.value = password;

    // Опционально: подсветим поле
    passwordInput.style.backgroundColor = '#e8f5e8';
    setTimeout(() => {
        passwordInput.style.backgroundColor = '';
    }, 1000);
});

// --- Добавление папки ---
const addFolderBtn = document.getElementById('addFolderBtn');
const folderModal = document.getElementById('folderModal');
const addFolderForm = document.getElementById('addFolderForm');
const cancelFolderBtn = document.getElementById('cancelFolderBtn');
const saveFolderBtn = document.getElementById('saveFolderBtn'); // Нужно добавить в HTML

// Открытие модального окна
addFolderBtn.addEventListener('click', async () => {
    // Очистим форму
    document.getElementById('folderName').value = '';
    const select = document.getElementById('parentFolderId');
    select.innerHTML = '<option value="">Без родителя</option>';

    // Загрузим все папки как возможных родителей
    try {
        const res = await fetch('/api/folder', { headers: authHeaders }); 
        if (res.ok) {
            const folders = await res.json();
            folders.forEach(f => {
                const opt = document.createElement('option');
                opt.value = f.id;
                opt.textContent = f.name;
                select.appendChild(opt);
            });
        }
    } catch (err) {
        console.error('Ошибка загрузки папок для выбора родителя', err);
    }

    // Настройка модального окна на "Создать"
    folderModal.querySelector('h3').textContent = 'Новая папка';
    saveFolderBtn.textContent = 'Создать';
    saveFolderBtn.onclick = null;

    // Обработчик создания
    saveFolderBtn.onclick = async () => {
        const name = document.getElementById('folderName').value.trim();
        if (!name) {
            alert('Имя папки обязательно');
            return;
        }

        const data = {
            name: name,
            parentFolderId: document.getElementById('parentFolderId').value || null
        };

        const res = await fetch('/api/folder', { 
            method: 'POST',
            headers: authHeaders,
            body: JSON.stringify(data)
        });

        if (res.ok) {
            folderModal.classList.remove('show');
            loadFolders(); // Обновим список
        } else {
            alert('Ошибка создания папки');
        }

        // Сброс
        saveFolderBtn.onclick = null;
    };

    folderModal.classList.add('show');
});

// Закрытие модального окна
cancelFolderBtn.addEventListener('click', () => {
    folderModal.classList.remove('show');
    // Сброс обработчика
    if (saveFolderBtn) saveFolderBtn.onclick = null;
});

// --- Загрузка папок ---
async function loadFolders() {
    try {
        const res = await fetch('/api/folder', { headers: authHeaders });
        if (!res.ok) throw new Error();

        const folders = await res.json();
        foldersList.innerHTML = '';

        // Строим дерево
        const rootFolders = folders.filter(f => !f.parentFolderId);
        const folderMap = {};
        folders.forEach(f => folderMap[f.id] = { ...f, children: [] });

        // Заполняем детей
        folders.forEach(f => {
            if (f.parentFolderId) {
                const parent = folderMap[f.parentFolderId];
                if (parent) parent.children.push(folderMap[f.id]);
            }
        });

        // Рекурсивная отрисовка
        function renderFolder(folder, level = 0) {
            const li = document.createElement('li');
            li.style.paddingLeft = (level * 16) + 'px';
            li.style.display = 'flex';
            li.style.alignItems = 'center';
            li.style.gap = '8px';

            // Название папки (при клике — загружаем пароли)
            const nameSpan = document.createElement('span');
            nameSpan.textContent = folder.name;
            nameSpan.style.cursor = 'pointer';
            nameSpan.style.fontWeight = '500';
            nameSpan.addEventListener('click', () => {
                loadPasswords(folder.id);
            });
            li.appendChild(nameSpan);

            // Кнопка "Удалить" — отдельно, рядом
            const deleteBtn = document.createElement('button');
            deleteBtn.innerHTML = '🗑️'; 
            deleteBtn.style.background = 'none';
            deleteBtn.style.border = 'none';
            deleteBtn.style.cursor = 'pointer';
            deleteBtn.style.fontSize = '14px';
            deleteBtn.style.opacity = '0'; 
            deleteBtn.style.transition = 'opacity 0.2s';

            // Показывать только при наведении на li
            li.addEventListener('mouseenter', () => deleteBtn.style.opacity = '1');
            li.addEventListener('mouseleave', () => deleteBtn.style.opacity = '0');

            deleteBtn.addEventListener('click', async (e) => {
                e.stopPropagation(); // 🔥 Останавливаем всплытие, чтобы не сработал клик на папку

                if (confirm(`Удалить папку "${folder.name}" и всё содержимое?`)) {
                    const res = await fetch(`/api/folder/${folder.id}`, {
                        method: 'DELETE',
                        headers: authHeaders
                    });

                    if (res.ok) {
                        li.remove(); // Удаляем папку из интерфейса
                        if (currentFolderId === folder.id) {
                            loadPasswords(null); // Если была выбрана — показываем все пароли
                        }
                    } else {
                        alert('Ошибка при удалении папки');
                    }
                }
            });

            li.appendChild(deleteBtn);
            foldersList.appendChild(li);

            // Рекурсивно отрисовываем подпапки
            folder.children.forEach(child => renderFolder(child, level + 1));
        }

        rootFolders.forEach(f => renderFolder(folderMap[f.id]));

        // Загружаем все пароли
        loadPasswords(null);
    } catch (err) {
        alert('Ошибка загрузки папок');
    }
}
const allFoldersBtn = document.getElementById('allFoldersBtn');

allFoldersBtn.addEventListener('click', () => {
    // Снимаем выделение со всех папок
    document.querySelectorAll('.folder-item').forEach(el => el.classList.remove('active'));

    // Выделяем кнопку "Все пароли"
    allFoldersBtn.classList.add('active');

    // Загружаем все пароли
    loadPasswords(null);
});
async function loadPasswords(folderId) {
    // Снимаем активное состояние с кнопки "Все пароли"
    if (allFoldersBtn) {
        allFoldersBtn.classList.remove('active');
    }

    // Снимаем выделение с папок
    document.querySelectorAll('.folder-item').forEach(el => el.classList.remove('active'));
    if (folderId) {
        document.querySelector(`.folder-item[data-id="${folderId}"]`).classList.add('active');
    }

    try {
        const res = await fetch(`/api/password?folderId=${folderId || ''}`, { headers: authHeaders });
        if (!res.ok) throw new Error();

        const passwords = await res.json();
        passwordsList.innerHTML = '';
        passwords.forEach(p => addPasswordCard(p));
    } catch (err) {
        alert('Ошибка загрузки паролей');
    }
}
const searchInput = document.getElementById('searchInput');

// Глобальная переменная для хранения всех паролей
let allPasswords = [];
let currentFolderId = null;

// loadPasswords, чтобы сохранять все данные
async function loadPasswords(folderId) {
    currentFolderId = folderId;

    try {
        const res = await fetch('/api/password', { headers: authHeaders });
        if (!res.ok) throw new Error();

        allPasswords = await res.json();

        // Применяем фильтрацию
        applyFilter();
    } catch (err) {
        alert('Ошибка загрузки паролей');
    }
}

// Фильтрация: по папке и поиску
function applyFilter() {
    const query = searchInput.value.toLowerCase().trim();

    let filtered = allPasswords;

    // Фильтр по папке
    if (currentFolderId !== null) {
        filtered = filtered.filter(p => p.folderId === currentFolderId);
    }

    // Фильтр по поиску
    if (query) {
        filtered = filtered.filter(p =>
            p.title.toLowerCase().includes(query) ||
            p.username.toLowerCase().includes(query) ||
            (p.notes && p.notes.toLowerCase().includes(query))
        );
    }

    // Отрисовка
    passwordsList.innerHTML = '';
    filtered.forEach(p => addPasswordCard(p));
}

// Обработчик поиска
searchInput.addEventListener('input', applyFilter);
//Добваление пароля
function addPasswordCard(p) {
    const div = document.createElement('div');
    div.className = 'password-card';
    div.innerHTML = `
        <h4>${p.title}</h4>
        <p><strong>Логин:</strong> ${p.username}</p>
        <p><strong>Сайт:</strong> <a href="${p.url}" target="_blank">${p.url}</a></p>
        <p><strong>Пароль:</strong> <span class="password-mask">••••••••</span> <button class="show-pass">Показать</button></p>
        <p><strong>Заметки:</strong> ${p.notes || '—'}</p>
        <div style="text-align: right; margin-top: 10px;">
            <button class="edit-pass" data-id="${p.id}">Редактировать</button>
            <button class="delete-pass" data-id="${p.id}">Удалить</button>
        </div>
    `;
    passwordsList.appendChild(div);

    // --- Показ пароля ---
    div.querySelector('.show-pass').addEventListener('click', async function () {
        const span = this.previousElementSibling;
        if (span.textContent === '••••••••') {
            if (!masterPassword) {
                const entered = prompt('Введите мастер-пароль:');
                if (!entered) return;
                masterPassword = entered;
            }
            try {
                const cleanEncrypted = p.encryptedPassword.replace(/["']/g, '').trim();
                const plainText = await decrypt(cleanEncrypted, masterPassword);
                span.textContent = plainText;
                this.textContent = 'Скрыть';
            } catch (e) {
                alert('Не удалось расшифровать пароль. Проверьте мастер-пароль.');
                console.error(e);
                masterPassword = '';
            }
        } else {
            span.textContent = '••••••••';
            this.textContent = 'Показать';
        }
    });

    // --- Редактирование ---
    div.querySelector('.edit-pass').addEventListener('click', async function () {
        const passwordId = parseInt(this.dataset.id);
        console.log('Редактируем пароль с id:', passwordId);

        if (!masterPassword) {
            const entered = prompt('Введите мастер-пароль для редактирования:');
            if (!entered) return;
            masterPassword = entered;
        }

        try {
            const res = await fetch(`/api/password`, { headers: authHeaders });
            if (!res.ok) throw new Error('Ошибка загрузки');
            const passwords = await res.json();
            const p = passwords.find(p => p.id == passwordId);
            if (!p) return;

            // Заполняем форму
            document.getElementById('passwordTitle').value = p.title;
            document.getElementById('passwordUsername').value = p.username;
            document.getElementById('passwordUrl').value = p.url || '';
            document.getElementById('passwordNotes').value = p.notes || '';
            document.getElementById('passwordFolderId').value = p.folderId || '';

            const passInput = document.getElementById('passwordValue');
            passInput.required = false;
            passInput.placeholder = 'Оставить пустым, чтобы не менять';

            // Меняем заголовок
            passwordModal.querySelector('h3').textContent = 'Редактировать пароль';
            savePasswordBtn.textContent = 'Сохранить';

            // Устанавливаем обработчик на кнопку "Сохранить"
            savePasswordBtn.onclick = async () => {
                const data = {
                    title: document.getElementById('passwordTitle').value,
                    username: document.getElementById('passwordUsername').value,
                    password: document.getElementById('passwordValue').value || undefined,
                    masterPassword: masterPassword,
                    url: document.getElementById('passwordUrl').value || null,
                    notes: document.getElementById('passwordNotes').value || null,
                    folderId: document.getElementById('passwordFolderId').value || null
                };

                const res = await fetch(`/api/password/${passwordId}`, {
                    method: 'PUT',
                    headers: authHeaders,
                    body: JSON.stringify(data)
                });

                if (res.ok) {
                    passwordModal.classList.remove('show');
                    loadPasswords(null); // Обновляем список
                } else {
                    alert('Ошибка при редактировании');
                }

                // Сброс
                savePasswordBtn.onclick = null;
                passInput.required = true;
                passInput.placeholder = 'Пароль';
                passwordModal.querySelector('h3').textContent = 'Добавить пароль';
                savePasswordBtn.textContent = 'Создать';
            };

            passwordModal.classList.add('show');
        } catch (e) {
            console.error(e);
            alert('Ошибка при редактировании');
        }
    });

    // --- Удаление ---
    div.querySelector('.delete-pass').addEventListener('click', async function () {
        if (!confirm('Удалить пароль?')) return;

        const res = await fetch(`/api/password/${p.id}`, {
            method: 'DELETE',
            headers: authHeaders
        });

        if (res.ok) {
            div.remove();
        } else {
            alert('Ошибка удаления');
        }
    });
}


// Выход
logoutBtn.addEventListener('click', () => {
    localStorage.removeItem('token');
    window.location.href = '/auth.html';
});

// Загружаем данные
loadFolders();