const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');

// Функция для показа сообщений
function showMessage(elementId, message, type = 'error') {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        element.style.display = 'block';
        element.style.backgroundColor = type === 'error' ? '#fee' : '#efe';
        element.style.color = type === 'error' ? '#c33' : '#393';
        element.style.padding = '12px';
        element.style.borderRadius = '8px';
        element.style.marginBottom = '15px';
        element.style.textAlign = 'center';

        setTimeout(() => {
            element.style.display = 'none';
        }, 5000);
    } else {
        alert(message); // Fallback
    }
}

// Создаем элементы для сообщений, если их нет
if (!document.getElementById('loginMessage')) {
    const loginMsg = document.createElement('div');
    loginMsg.id = 'loginMessage';
    loginMsg.style.display = 'none';
    loginForm.insertBefore(loginMsg, loginForm.firstChild);
}

if (!document.getElementById('registerMessage')) {
    const registerMsg = document.createElement('div');
    registerMsg.id = 'registerMessage';
    registerMsg.style.display = 'none';
    registerForm.insertBefore(registerMsg, registerForm.firstChild);
}

// Переключение вкладок
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.auth-form').forEach(f => f.classList.remove('active'));

        tab.classList.add('active');
        document.getElementById(tab.dataset.tab === 'login' ? 'loginForm' : 'registerForm').classList.add('active');

        // Очищаем сообщения при переключении
        document.getElementById('loginMessage').style.display = 'none';
        document.getElementById('registerMessage').style.display = 'none';
    });
});

// --- Вход ---
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    // Показываем индикатор загрузки
    const submitBtn = loginForm.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Вход...';
    submitBtn.disabled = true;

    try {
        const formData = new FormData(loginForm);
        const data = Object.fromEntries(formData);

        console.log('Login attempt with:', { email: data.email, password: '***' });

        const res = await fetch('/api/auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });

        console.log('Login response status:', res.status);

        const responseText = await res.text();
        console.log('Login response text:', responseText);

        let result;
        try {
            result = responseText ? JSON.parse(responseText) : {};
        } catch (parseError) {
            console.error('JSON parse error:', parseError);
            throw new Error('Некорректный ответ от сервера');
        }

        if (res.ok) {
            console.log('Login successful:', result);

            if (result.token) {
                localStorage.setItem('token', result.token);
                if (result.email) {
                    localStorage.setItem('user_email', result.email);
                }

                showMessage('loginMessage', '✅ Вход успешен! Перенаправление...', 'success');

                // Небольшая задержка перед перенаправлением
                setTimeout(() => {
                    window.location.href = 'index.html';
                }, 1000);
            } else {
                throw new Error('Токен не получен от сервера');
            }
        } else {
            console.error('Login failed:', result);
            const errorMsg = result.message || `Ошибка ${res.status}: ${res.statusText}`;
            showMessage('loginMessage', `❌ ${errorMsg}`, 'error');
        }
    } catch (err) {
        console.error('Login error:', err);
        showMessage('loginMessage', `❌ Ошибка: ${err.message}`, 'error');
    } finally {
        // Восстанавливаем кнопку
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
    }
});

// --- Регистрация ---
registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const submitBtn = registerForm.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Регистрация...';
    submitBtn.disabled = true;

    try {
        const formData = new FormData(registerForm);
        const data = Object.fromEntries(formData);

        // Проверка пароля
        if (data.password.length < 6) {
            showMessage('registerMessage', '❌ Пароль должен быть не менее 6 символов', 'error');
            return;
        }

        console.log('Register attempt with:', { email: data.email, password: '***' });

        const res = await fetch('/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });

        console.log('Register response status:', res.status);

        const responseText = await res.text();
        console.log('Register response text:', responseText);

        let result;
        try {
            result = responseText ? JSON.parse(responseText) : {};
        } catch (parseError) {
            console.error('JSON parse error:', parseError);
            throw new Error('Некорректный ответ от сервера');
        }

        if (res.ok) {
            console.log('Registration successful:', result);
            showMessage('registerMessage', '✅ Регистрация успешна! Теперь войдите в систему.', 'success');

            // Автоматически переключаемся на вкладку входа
            setTimeout(() => {
                document.querySelector('[data-tab="login"]').click();
                document.getElementById('loginForm').querySelector('input[name="email"]').value = data.email;
            }, 1500);
        } else {
            console.error('Registration failed:', result);
            const errorMsg = result.message || `Ошибка ${res.status}: ${res.statusText}`;
            showMessage('registerMessage', `❌ ${errorMsg}`, 'error');
        }
    } catch (err) {
        console.error('Registration error:', err);
        showMessage('registerMessage', `❌ Ошибка: ${err.message}`, 'error');
    } finally {
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
    }
});

// Функция выхода
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user_email');
    window.location.href = 'auth.html';
}

// Открываем консоль для отладки (только в development)
if (window.location.hostname === 'localhost') {
    console.log('Auth script loaded successfully');
    console.log('Current URL:', window.location.href);
}