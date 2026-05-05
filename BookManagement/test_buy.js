const http = require('http');

async function test() {
  try {
    const res = await fetch('http://localhost:5104/api/auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ userName: 'testuser', email: 'testuser@test.com', password: 'Password123', fullName: 'Test User' })
    });
    console.log('Register:', await res.text());

    const loginRes = await fetch('http://localhost:5104/api/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ identifier: 'testuser', password: 'Password123' })
    });
    const loginData = await loginRes.json();
    console.log('Login:', loginData.token ? 'Success' : loginData);

    const depositRes = await fetch('http://localhost:5104/api/transactions/deposit', {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${loginData.token}`
      },
      body: JSON.stringify({ amount: 1000000 })
    });
    console.log('Deposit:', await depositRes.text());

    const buyRes = await fetch('http://localhost:5104/api/transactions', {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${loginData.token}`
      },
      body: JSON.stringify({ bookId: 1, type: 0 })
    });
    console.log('Buy:', await buyRes.text());

  } catch (e) {
    console.error(e);
  }
}
test();
