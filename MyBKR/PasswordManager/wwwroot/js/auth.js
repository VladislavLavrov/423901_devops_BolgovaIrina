const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');

// Переключение вкладок
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.auth-form').forEach(f => f.classList.remove('active'));

        tab.classList.add('active');
        document.getElementById(tab.dataset.tab === 'login' ? 'loginForm' : 'registerForm').classList.add('active');
    });
});

// --- Вход ---
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = new FormData(loginForm);
    const data = Object.fromEntries(formData);

    try {
        const res = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (res.ok) {
            const result = await res.json();
            localStorage.setItem('token', result.token);
            // ✅ Перенаправление на главную
            window.location.href = '/index.html';
        } else {
            const error = await res.json().catch(() => ({}));
            alert(error.message || 'Неверный email или пароль');
        }
    } catch (err) {
        console.error(err);
        alert('Ошибка сети или сервера');
    }
});

// --- Регистрация ---
registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    const formData = new FormData(registerForm);
    const data = Object.fromEntries(formData);

    try {
        const res = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });

        if (res.ok) {
            alert('Регистрация успешна! Войдите в систему.');
            // Переключаемся на вкладку входа
            document.querySelector('[data-tab="login"]').click();
        } else {
            const error = await res.json().catch(() => ({}));
            alert(error.message || 'Ошибка регистрации');
        }
    } catch (err) {
        console.error(err);
        alert('Ошибка сети или сервера');
    }
});