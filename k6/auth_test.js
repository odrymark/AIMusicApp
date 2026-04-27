import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { SharedArray } from 'k6/data';

const errorRate = new Rate('errors');
const loginDuration = new Trend('login_duration');
const refreshDuration = new Trend('refresh_duration');

const BASE_URL = __ENV.BASE_URL || 'http://167.86.77.173:8080';

const users = new SharedArray('users', function () {
  return [
    { username: 'testuser1',  password: 'testpassword' },
    { username: 'testuser2',  password: 'testpassword' },
    { username: 'testuser3',  password: 'testpassword' },
    { username: 'testuser4',  password: 'testpassword' },
    { username: 'testuser5',  password: 'testpassword' },
    { username: 'testuser6',  password: 'testpassword' },
    { username: 'testuser7',  password: 'testpassword' },
    { username: 'testuser8',  password: 'testpassword' },
    { username: 'testuser9',  password: 'testpassword' },
    { username: 'testuser10', password: 'testpassword' },
    { username: 'testuser11', password: 'testpassword' },
    { username: 'testuser12', password: 'testpassword' },
    { username: 'testuser13', password: 'testpassword' },
    { username: 'testuser14', password: 'testpassword' },
    { username: 'testuser15', password: 'testpassword' },
    { username: 'testuser16', password: 'testpassword' },
    { username: 'testuser17', password: 'testpassword' },
    { username: 'testuser18', password: 'testpassword' },
    { username: 'testuser19', password: 'testpassword' },
    { username: 'testuser20', password: 'testpassword' },
  ];
});

export const options = {
  scenarios: {
    login_load: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '1m',  target: 10 },
        { duration: '30s', target: 0  },
      ],
    },
    refresh_load: {
      executor: 'ramping-vus',
      startVUs: 0,
      startTime: '2m10s',
      stages: [
        { duration: '30s', target: 20 },
        { duration: '1m',  target: 20 },
        { duration: '30s', target: 0  },
      ],
    },
  },
  thresholds: {
    http_req_failed:  ['rate<0.04'],
    login_duration:   ['p(95)<2000'],
    refresh_duration: ['p(95)<1000'],
    errors:           ['rate<0.04'],
  },
};

export default function () {
  const user = users[(__VU - 1) % users.length];

  const loginRes = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify(user),
    { headers: { 'Content-Type': 'application/json' } }
  );

  loginDuration.add(loginRes.timings.duration);

  const loginOk = check(loginRes, {
    'login status is 200':    (r) => r.status === 200,
    'login returns username': (r) => r.json('username') !== '',
  });

  errorRate.add(!loginOk);

  if (!loginOk) {
    console.error(`Login failed [${loginRes.status}]: ${loginRes.body}`);
    sleep(1);
    return;
  }

  sleep(1);

  const refreshRes = http.post(
    `${BASE_URL}/api/auth/refresh`,
    null,
    { headers: { 'Content-Type': 'application/json' } }
  );

  refreshDuration.add(refreshRes.timings.duration);

  const refreshOk = check(refreshRes, {
    'refresh status is 200': (r) => r.status === 200,
  });

  errorRate.add(!refreshOk);

  if (!refreshOk) {
    console.error(`Refresh failed [${refreshRes.status}]: ${refreshRes.body}`);
    sleep(1);
    return;
  }

  sleep(1);

  // Logout — jwt and refreshToken cookies sent automatically
  const logoutRes = http.post(
    `${BASE_URL}/api/auth/logout`,
    null,
    { headers: { 'Content-Type': 'application/json' } }
  );

  check(logoutRes, {
    'logout status is 200': (r) => r.status === 200,
  });

  sleep(1);
}
