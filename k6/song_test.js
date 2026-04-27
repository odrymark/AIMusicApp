import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { SharedArray } from 'k6/data';

const errorRate = new Rate('errors');
const getSongsDuration = new Trend('get_songs_duration');
const getUserSongsDuration = new Trend('get_user_songs_duration');
const getSignedUrlDuration = new Trend('get_signed_url_duration');

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
    public_songs_load: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 10 },
        { duration: '1m',  target: 10 },
        { duration: '30s', target: 0  },
      ],
    },
    authenticated_songs_load: {
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
    http_req_failed:         ['rate<0.01'],
    get_signed_url_duration: ['p(95)<400'],
    get_songs_duration:      ['p(95)<500'],
    get_user_songs_duration: ['p(95)<450'],
    errors:                  ['rate<0.01'],
  }, 
};

export default function () {
  const user = users[(__VU - 1) % users.length];

  const getSongsRes = http.get(`${BASE_URL}/api/song/getSongs`);

  getSongsDuration.add(getSongsRes.timings.duration);

  const getSongsOk = check(getSongsRes, {
    'getSongs status is 200': (r) => r.status === 200,
    'getSongs returns array': (r) => r.status === 200 && Array.isArray(r.json()),
  });

  errorRate.add(!getSongsOk);

  if (!getSongsOk) {
    console.error(`getSongs failed [${getSongsRes.status}]: ${getSongsRes.body}`);
    sleep(1);
    return;
  }

  sleep(1);

  const loginRes = http.post(
    `${BASE_URL}/api/auth/login`,
    JSON.stringify(user),
    { headers: { 'Content-Type': 'application/json' } }
  );

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

  const userSongsRes = http.get(`${BASE_URL}/api/song/getUserSongs`);

  getUserSongsDuration.add(userSongsRes.timings.duration);

  const userSongsOk = check(userSongsRes, {
    'getUserSongs status is 200': (r) => r.status === 200,
    'getUserSongs returns array': (r) => r.status === 200 && Array.isArray(r.json()),
  });

  errorRate.add(!userSongsOk);

  if (!userSongsOk) {
    console.error(`getUserSongs failed [${userSongsRes.status}]: ${userSongsRes.body}`);
    sleep(1);
    return;
  }

  sleep(1);

  const songs = userSongsRes.json();

  if (Array.isArray(songs) && songs.length > 0) {
    const songKey = songs[0].songKey;
    const signedUrlRes = http.get(
      `${BASE_URL}/api/song/getSignedUrl?key=${encodeURIComponent(songKey)}`
    );

    getSignedUrlDuration.add(signedUrlRes.timings.duration);

    const signedUrlOk = check(signedUrlRes, {
      'getSignedUrl status is 200':    (r) => r.status === 200,
      'getSignedUrl returns a string': (r) => r.body.startsWith('https://'),
    });

    errorRate.add(!signedUrlOk);

    if (!signedUrlOk) {
      console.error(`getSignedUrl failed [${signedUrlRes.status}]: ${signedUrlRes.body}`);
    }
  }

  sleep(1);
}
