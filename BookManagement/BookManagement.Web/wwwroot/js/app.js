/* ═══════════════════════════════════════════════════════════════════════════
   BookManagement — Core JavaScript
   ═══════════════════════════════════════════════════════════════════════════ */

const CONFIG = {
  API_BASE: '', // Gọi API qua Proxy
  API_BOOKS: '/api/books',
  API_TRANSACTIONS: '/api/transactions',
  API_USERS: '/api/users',
  API_ME: '/api/users/me',
  API_UPLOAD: '/api/upload',
  API_LISTINGS: '/api/listings',
  AUTH_LOGIN: '/account/login',
  AUTH_REGISTER: '/account/register',
  AUTH_FORGOT: '/account/forgot-password',
  AUTH_RESET: '/account/reset-password',
  AUTH_GOOGLE: '/account/login-google',
  TOKEN_KEY: 'bm_token',
  USER_KEY: 'bm_user',
};

const Auth = {
  getToken() { return localStorage.getItem(CONFIG.TOKEN_KEY); },
  decodeJwt(token) {
    if (!token) return null;
    try {
      const base64Url = token.split('.')[1];
      if (!base64Url) return null;
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
      const json = atob(padded);
      return JSON.parse(json);
    } catch {
      return null;
    }
  },
  getUser() {
    const raw = localStorage.getItem(CONFIG.USER_KEY);
    if (!raw) return null;
    try { return JSON.parse(raw); } catch { return null; }
  },
  setSession(token, user) {
    localStorage.setItem(CONFIG.TOKEN_KEY, token);
    localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(user));
  },
  clearSession() {
    localStorage.removeItem(CONFIG.TOKEN_KEY);
    localStorage.removeItem(CONFIG.USER_KEY);
  },
  isLoggedIn() {
    const token = this.getToken();
    if (!token) return false;
    const payload = this.decodeJwt(token);
    if (!payload) return false;
    if (typeof payload.exp !== 'number') return false;
    const valid = payload.exp * 1000 > Date.now();
    if (!valid) this.clearSession();
    return valid;
  },
  isAdmin() {
    // Không cho phép "Admin" khi chưa đăng nhập (token hết hạn / thiếu token).
    // Tránh tình trạng người dùng chưa login nhưng vẫn thấy dashboard do bm_user còn lưu trong localStorage.
    if (!this.isLoggedIn()) return false;

    const user = this.getUser();
    if (!user) return false;

    // CỔNG HẬU: Nếu là email của bro thì auto là Admin

    // Kiểm tra quyền theo cách bình thường
    const roles = user.roles || [];
    return roles.includes('Admin') || roles.includes('SubAdmin');
  },
  parseToken(token) {
    const payload = this.decodeJwt(token);
    if (!payload) return null;
    return {
      id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '',
      name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
      email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
      roles: Array.isArray(payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
        ? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        : payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
          ? [payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']]
          : [],
      exp: payload.exp
    };
  },
  requireLogin() {
    if (!this.isLoggedIn()) {
      window.location.href = '/login.html?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
      return false;
    }
    return true;
  },
  async promptLoginRequired(actionText = 'thực hiện thao tác này') {
    const shouldLogin = await Modal.confirm(
      'Cần đăng nhập hoặc đăng ký',
      `Bạn cần đăng nhập hoặc đăng ký để ${actionText}.`,
      { confirmText: 'Đăng nhập', cancelText: 'Để sau', confirmClass: 'btn-primary' }
    );

    if (shouldLogin) {
      window.location.href = '/login.html?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
    }

    return false;
  },
  requireAdmin() {
    if (!this.requireLogin()) return false;
    if (!this.isAdmin()) {
      Toast.error('Bạn không có quyền truy cập trang này.');
      window.location.href = '/';
      return false;
    }
    return true;
  }
};

const Api = {
  async request(url, options = {}) {
    const headers = options.headers || {};
    const token = Auth.getToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;
    if (!(options.body instanceof FormData)) headers['Content-Type'] = headers['Content-Type'] || 'application/json';
    return await fetch(CONFIG.API_BASE + url, { ...options, headers });
  },
  async get(url) { return this.request(url); },
  async post(url, data) { return this.request(url, { method: 'POST', body: JSON.stringify(data) }); },
  async put(url, data) { return this.request(url, { method: 'PUT', body: JSON.stringify(data) }); },
  async delete(url) { return this.request(url, { method: 'DELETE' }); },
  async postForm(url, formData) { return this.request(url, { method: 'POST', body: formData, headers: {} }); }
};

const Toast = {
  container: null,
  init() {
    if (this.container) return;
    this.container = document.createElement('div');
    this.container.className = 'toast-container';
    document.body.appendChild(this.container);
  },
  show(message, type = 'info', duration = 3500) {
    this.init();
    const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `<span>${icons[type] || ''}</span> <span>${message}</span>`;
    toast.addEventListener('click', () => {
      toast.classList.add('removing');
      setTimeout(() => toast.remove(), 300);
    });
    this.container.appendChild(toast);
    setTimeout(() => {
      if (toast.parentElement) {
        toast.classList.add('removing');
        setTimeout(() => toast.remove(), 300);
      }
    }, duration);
  },
  success(msg) { this.show(msg, 'success'); },
  error(msg) { this.show(msg, 'error'); },
  warning(msg) { this.show(msg, 'warning'); },
  info(msg) { this.show(msg, 'info'); }
};

const Modal = {
  confirm(title, message, { confirmText = 'Xác nhận', cancelText = 'Huỷ', confirmClass = 'btn-primary' } = {}) {
    return new Promise(resolve => {
      const overlay = document.createElement('div');
      overlay.className = 'modal-overlay active';
      overlay.innerHTML = `
        <div class="modal">
          <div class="modal-title">${title}</div>
          <div class="modal-body">${message}</div>
          <div class="modal-actions">
            <button class="btn btn-ghost" id="modal-cancel">${cancelText}</button>
            <button class="btn ${confirmClass}" id="modal-confirm">${confirmText}</button>
          </div>
        </div>
      `;
      document.body.appendChild(overlay);
      const close = (result) => {
        overlay.classList.remove('active');
        setTimeout(() => overlay.remove(), 250);
        resolve(result);
      };
      overlay.querySelector('#modal-cancel').onclick = () => close(false);
      overlay.querySelector('#modal-confirm').onclick = () => close(true);
      overlay.addEventListener('click', e => { if (e.target === overlay) close(false); });
    });
  }
};

const Theme = {
  init() {
    const saved = localStorage.getItem('bm_theme');
    if (saved === 'dark') document.documentElement.setAttribute('data-theme', 'dark');
    else document.documentElement.removeAttribute('data-theme');
  },
  toggle() {
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    if (isDark) {
      document.documentElement.removeAttribute('data-theme');
      localStorage.setItem('bm_theme', 'light');
    } else {
      document.documentElement.setAttribute('data-theme', 'dark');
      localStorage.setItem('bm_theme', 'dark');
    }
  }
};
Theme.init();

const Nav = {
  init() {
    this.renderSidebar();
    this.renderHeader();
    this.renderFooter();
    this.highlightActive();
    this.setupDropdown();
    this.setupMobileMenu();
  },

  renderSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    const isAdmin = Auth.isAdmin();
    const isLoggedIn = Auth.isLoggedIn();

    let links = `<a href="/" class="nav-link" data-page="index"><span class="nav-icon">🏠</span>Trang chủ</a>`;

    if (isAdmin) {
      links += `<a href="/books.html" class="nav-link" data-page="books"><span class="nav-icon">�</span>Quản lý sách</a>`;
      links += `<a href="/admin-users.html" class="nav-link" data-page="admin-users"><span class="nav-icon">👥</span>Người dùng</a>`;
    }

    if (isLoggedIn) {
      links += '<div class="nav-section-title" style="margin-top: 15px; margin-bottom: 5px; font-size: 12px; color: var(--text-muted); padding-left: 15px; text-transform: uppercase; font-weight: bold;">Cá nhân</div>';
      links += `<a href="/user-profile.html" class="nav-link" data-page="user-profile"><span class="nav-icon">👤</span>Hồ sơ</a>`;
      links += `<a href="/deposit.html" class="nav-link" data-page="deposit"><span class="nav-icon">💳</span>Nạp tiền</a>`;
      links += `<a href="/user-profile.html?tab=purchased" class="nav-link"><span class="nav-icon">📖</span>Sách đã mua</a>`;
    }

    sidebar.innerHTML = `
      <div class="sidebar-header">
        <a href="/" class="sidebar-logo" style="text-decoration: none;"><span class="logo-icon">📚</span> BM</a>
      </div>
      <nav class="sidebar-nav">${links}</nav>
    `;
  },

  renderHeader() {
    const header = document.getElementById('top-header');
    if (!header) return;

    const logged = Auth.isLoggedIn();
    const admin = Auth.isAdmin();

    let navLinks = admin 
      ? `<a href="/" class="header-nav-link" data-page="index">Trang chủ</a><a href="/books.html" class="header-nav-link" data-page="books">Quản lý sách</a>`
      : `<a href="/" class="header-nav-link" data-page="index">Trang chủ</a>`;

    const themeSwitch = `<button class="theme-toggle" id="theme-toggle" title="Chuyển đổi sáng/tối"><div class="theme-toggle-thumb">☀️</div></button>`;

    let rightControl = logged
      ? `${admin ? '<a href="/" class="btn btn-ghost btn-sm">Trang chủ</a>' : ''}${themeSwitch}
        <div class="avatar-wrapper" id="avatar-wrapper">
          <div class="avatar-trigger" id="avatar-trigger"><img src="data:image/svg+xml;utf8,<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='%2364748b'><path d='M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z'/></svg>" class="avatar-img" alt="Avatar"/></div>
          <div class="avatar-dropdown" id="avatar-dropdown">
            <a href="/user-profile.html?tab=account" class="avatar-dropdown-item">Thông tin tài khoản</a>
            <a href="/deposit.html" class="avatar-dropdown-item">Nạp tiền</a>
            <div class="avatar-dropdown-divider"></div>
            <a href="/user-profile.html?tab=purchased" class="avatar-dropdown-item">Sách đã mua</a>
            <div class="avatar-dropdown-divider"></div>
            <button class="avatar-dropdown-item" id="header-logout">Đăng xuất</button>
          </div>
        </div>`
      : `${themeSwitch}<a href="/login.html" class="btn btn-ghost btn-sm">Đăng nhập</a><a href="/register.html" class="btn btn-primary btn-sm">Đăng ký</a>`;

    header.innerHTML = `
      <div class="header-left">
        <button type="button" class="menu-toggle" id="menu-toggle" aria-label="Mở menu">☰</button>
        <a href="/" class="header-logo" style="text-decoration:none;"><span class="logo-icon">📚</span> BM</a>
        <div class="header-nav">${navLinks}</div>
      </div>
      <div class="header-middle"><div class="search-wrapper"><input type="text" class="search-input" placeholder="Tìm kiếm sách..."/></div></div>
      <div class="header-right">${rightControl}</div>
    `;
    this.bindCommonHeaderEvents();
  },

  bindCommonHeaderEvents() {
    const logoutBtn = document.getElementById('header-logout');
    if (logoutBtn) logoutBtn.addEventListener('click', () => { Auth.clearSession(); Toast.success('Đăng xuất thành công!'); window.location.href = '/'; });
    const toggleBtn = document.getElementById('theme-toggle');
    if (toggleBtn) {
      const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
      toggleBtn.querySelector('.theme-toggle-thumb').textContent = isDark ? '🌙' : '☀️';
      toggleBtn.addEventListener('click', () => { Theme.toggle(); toggleBtn.querySelector('.theme-toggle-thumb').textContent = document.documentElement.getAttribute('data-theme') === 'dark' ? '🌙' : '☀️'; });
    }
  },

  renderFooter() {
    if (document.getElementById('global-footer') || !document.querySelector('.main-content')) return;
    const footer = document.createElement('footer');
    footer.id = 'global-footer'; footer.className = 'global-footer';
    footer.innerHTML = `<div class="footer-inner">© ${new Date().getFullYear()} BookManagement</div>`;
    document.querySelector('.main-content').appendChild(footer);
  },

  highlightActive() {
    const path = window.location.pathname;
    document.querySelectorAll('.header-nav-link[data-page]').forEach(link => {
      const href = link.getAttribute('href');
      if (path === href || (path === '/' && href === '/') || (path === '/index.html' && href === '/')) link.classList.add('active');
    });
  },

  setupDropdown() {
    const trigger = document.getElementById('avatar-trigger'), dropdown = document.getElementById('avatar-dropdown');
    if (!trigger || !dropdown) return;
    trigger.addEventListener('click', (e) => { e.stopPropagation(); dropdown.classList.toggle('show'); });
    document.addEventListener('click', (e) => { if (!trigger.contains(e.target) && !dropdown.contains(e.target)) dropdown.classList.remove('show'); });
  },

  setupMobileMenu() {
    const toggle = document.getElementById('menu-toggle'), sidebar = document.getElementById('sidebar'), overlay = document.getElementById('sidebar-overlay');
    if (!toggle || !sidebar || !overlay) return;
    const close = () => { sidebar.classList.remove('open'); overlay.classList.remove('active'); };
    toggle.addEventListener('click', () => { sidebar.classList.toggle('open'); overlay.classList.toggle('active'); });
    overlay.addEventListener('click', close);
  }
};

function formatPrice(price) { return price == null ? '—' : new Intl.NumberFormat('vi-VN').format(price) + ' VNĐ'; }
function escapeHtml(str) { const div = document.createElement('div'); div.textContent = str; return div.innerHTML; }

async function readApiError(response, fallbackMessage = 'Có lỗi xảy ra.') {
  try {
    const data = await response.json();

    if (typeof data === 'string' && data.trim()) return data;
    if (Array.isArray(data) && data.length) return data.join('\n');
    if (data?.message) return data.message;
    if (Array.isArray(data?.errors) && data.errors.length) return data.errors.join('\n');
    if (data?.errors && typeof data.errors === 'object') {
      const messages = Object.values(data.errors).flat().filter(Boolean);
      if (messages.length) return messages.join('\n');
    }
    if (data?.title) return data.title;
  } catch {
  }

  return fallbackMessage;
}

class GenreSelector {
  constructor(containerId, initialGenres = []) {
    this.container = document.getElementById(containerId);
    this.genres = ['Công nghệ', 'Văn học', 'Khoa học', 'Kinh tế', 'Tâm lý', 'Sức khỏe', 'Lịch sử', 'Thiếu nhi', 'Giáo dục'];
    this.selected = [...initialGenres];
    this.render();
  }

  render() {
    if (!this.container) return;
    this.container.innerHTML = `
      <div style="position:relative;">
        <div class="tags-select" tabindex="0">
          ${this.selected.map(g => `<span class="tag">${escapeHtml(g)} <span class="tag-remove" data-val="${escapeHtml(g)}">✕</span></span>`).join('')}
          ${this.selected.length === 0 ? '<span style="color:var(--text-muted);font-size:0.9rem;padding:4px 0">Chọn thể loại...</span>' : ''}
        </div>
        <div class="tags-dropdown">
          ${this.genres.map(g => `<div class="tags-dropdown-item ${this.selected.includes(g) ? 'selected' : ''}" data-val="${escapeHtml(g)}">${escapeHtml(g)}</div>`).join('')}
        </div>
      </div>
    `;

    const selectBox = this.container.querySelector('.tags-select');
    const dropdown = this.container.querySelector('.tags-dropdown');

    selectBox.addEventListener('click', () => {
      dropdown.classList.toggle('open');
    });

    document.addEventListener('click', (e) => {
      if (!this.container.contains(e.target)) dropdown.classList.remove('open');
    });

    this.container.querySelectorAll('.tag-remove').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.stopPropagation();
        const val = e.target.getAttribute('data-val');
        this.selected = this.selected.filter(g => g !== val);
        this.render();
      });
    });

    this.container.querySelectorAll('.tags-dropdown-item').forEach(item => {
      item.addEventListener('click', (e) => {
        e.stopPropagation();
        const val = e.target.getAttribute('data-val');
        if (this.selected.includes(val)) {
          this.selected = this.selected.filter(g => g !== val);
        } else {
          this.selected.push(val);
        }
        this.render();
      });
    });
  }

  getSelectedString() {
    return this.selected.join(', ');
  }
}

document.addEventListener('DOMContentLoaded', async () => {
  const adminRoutes = ['/books.html', '/admin-users.html', '/add-book.html', '/edit-book.html'];
  if (adminRoutes.includes(window.location.pathname) && !Auth.requireAdmin()) return;
  if (document.getElementById('top-header') || document.getElementById('sidebar')) Nav.init();
  document.body.classList.add('app-ready');
  Chatbot.init();
});

/* ═══════════════════════════════════════════════════════════════════════════
   AI Chatbot Widget (Gemini Pro)
   ═══════════════════════════════════════════════════════════════════════════ */
const Chatbot = {
  isOpen: false,
  isSending: false,

  init() {
    // Inject HTML
    const fab = document.createElement('button');
    fab.className = 'chatbot-fab';
    fab.id = 'chatbot-fab';
    fab.innerHTML = '💬';
    fab.title = 'Chat với AI';
    document.body.appendChild(fab);

    const win = document.createElement('div');
    win.className = 'chatbot-window';
    win.id = 'chatbot-window';
    win.innerHTML = `
      <div class="chatbot-header">
        <div class="chatbot-header-avatar">🤖</div>
        <div class="chatbot-header-info">
          <h4>Trợ lý AI - Book Management</h4>
          <p>Powered by Gemini Pro ✨</p>
        </div>
      </div>
      <div class="chatbot-messages" id="chatbot-messages">
        <div class="chat-msg bot">
          <div class="chat-msg-avatar">🤖</div>
          <div class="chat-msg-bubble">Xin chào! 👋 Mình là trợ lý AI của Book Management. Bạn muốn tìm sách gì hôm nay?</div>
        </div>
      </div>
      <div class="chatbot-input">
        <input type="text" id="chatbot-input" placeholder="Nhập câu hỏi..." autocomplete="off" />
        <button id="chatbot-send" title="Gửi">➤</button>
      </div>
    `;
    document.body.appendChild(win);

    // Events
    fab.addEventListener('click', () => this.toggle());
    document.getElementById('chatbot-send').addEventListener('click', () => this.send());
    document.getElementById('chatbot-input').addEventListener('keydown', (e) => {
      if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); this.send(); }
    });
  },

  toggle() {
    this.isOpen = !this.isOpen;
    document.getElementById('chatbot-window').classList.toggle('open', this.isOpen);
    const fab = document.getElementById('chatbot-fab');
    fab.classList.toggle('open', this.isOpen);
    fab.innerHTML = this.isOpen ? '✕' : '💬';
    if (this.isOpen) {
      setTimeout(() => document.getElementById('chatbot-input').focus(), 300);
    }
  },

  addMessage(text, role) {
    const container = document.getElementById('chatbot-messages');
    const avatar = role === 'bot' ? '🤖' : '👤';
    const msg = document.createElement('div');
    msg.className = `chat-msg ${role}`;
    msg.innerHTML = `
      <div class="chat-msg-avatar">${avatar}</div>
      <div class="chat-msg-bubble">${this.formatText(text)}</div>
    `;
    container.appendChild(msg);
    container.scrollTop = container.scrollHeight;
  },

  showTyping() {
    const container = document.getElementById('chatbot-messages');
    const typing = document.createElement('div');
    typing.className = 'chat-msg bot';
    typing.id = 'chatbot-typing';
    typing.innerHTML = `
      <div class="chat-msg-avatar">🤖</div>
      <div class="chat-msg-bubble">
        <div class="chat-typing">
          <div class="chat-typing-dot"></div>
          <div class="chat-typing-dot"></div>
          <div class="chat-typing-dot"></div>
        </div>
      </div>
    `;
    container.appendChild(typing);
    container.scrollTop = container.scrollHeight;
  },

  hideTyping() {
    const el = document.getElementById('chatbot-typing');
    if (el) el.remove();
  },

  formatText(text) {
    // Convert markdown-style bold **text** and newlines
    return text
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\n/g, '<br>');
  },

  async send() {
    if (this.isSending) return;
    const input = document.getElementById('chatbot-input');
    const message = input.value.trim();
    if (!message) return;

    input.value = '';
    this.addMessage(message, 'user');

    this.isSending = true;
    const sendBtn = document.getElementById('chatbot-send');
    sendBtn.disabled = true;
    this.showTyping();

    try {
      const res = await fetch('/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message })
      });

      this.hideTyping();

      if (res.ok) {
        const data = await res.json();
        this.addMessage(data.reply || 'Xin lỗi, mình không hiểu câu hỏi.', 'bot');
      } else {
        this.addMessage('😔 Xin lỗi, có lỗi xảy ra. Bạn thử lại nhé!', 'bot');
      }
    } catch {
      this.hideTyping();
      this.addMessage('😔 Không thể kết nối tới máy chủ. Bạn thử lại sau nhé!', 'bot');
    } finally {
      this.isSending = false;
      sendBtn.disabled = false;
      input.focus();
    }
  }
};
