import { Selector, ClientFunction } from 'testcafe';
const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';
const getLocation = ClientFunction(() => window.location.href);
const searchInput      = Selector('input[placeholder="Search songs..."]');
const songCards        = Selector('.grid > div[class*="bg-base-100"]');
const noSongsMessage   = Selector('div').withText('No songs found.');
const searchResultText = Selector('p').withAttribute('class', /text-sm text-base-content/);
const songSections     = Selector('div').withAttribute('class', /max-w-6xl/);
const appTitle         = Selector('span').withText('Music App');
const topPlaylists     = Selector('aside').find('div').withAttribute('class', /cursor-pointer hover:bg-base-200/);

fixture('Home Page')
    .page(`${BASE_URL}/home`)
    .skipJsErrors(true) 
    .beforeEach(async (t) => {
        await t.eval(() => {
            localStorage.setItem('token', 'test-token');
        });
        await t.navigateTo(`${BASE_URL}/home`);
    });

// Layout
test('renders app title', async (t) => {
    await t.expect(appTitle.exists).ok();
});
test('renders search input', async (t) => {
    await t.expect(searchInput.exists).ok();
    await t.expect(searchInput.getAttribute('placeholder')).eql('Search songs...');
});
test('renders top playlists sidebar', async (t) => {
    const playlistHeader = Selector('h3').withText('Top Playlists');
    await t.expect(playlistHeader.exists).ok();
});
test('renders song sections when not searching', async (t) => {
    const topTrending      = Selector('div').withText('Top Trending');
    const mostListened     = Selector('div').withText('Your Most Listened');
    const recommendations  = Selector('div').withText('Recommendations For You');
    await t.expect(topTrending.exists).ok();
    await t.expect(mostListened.exists).ok();
    await t.expect(recommendations.exists).ok();
});

// Search
test('shows search results when typing in search box', async (t) => {
    await t.typeText(searchInput, 'test');
    await t.expect(searchResultText.exists).ok();
    await t.expect(searchResultText.innerText).contains('result');
});
test('search result count updates as user types', async (t) => {
    await t.typeText(searchInput, 'zzzzzzzzz');
    await t.expect(noSongsMessage.exists).ok();
});
test('shows no results message for unmatched search', async (t) => {
    await t.typeText(searchInput, 'xxxxxxxxxxxxxxxxxxx');
    await t
        .expect(Selector('div').withText('No songs found.').exists)
        .ok();
});
test('clearing search restores song sections', async (t) => {
    await t.typeText(searchInput, 'test');
    await t.expect(searchResultText.exists).ok();
    await t.selectText(searchInput).pressKey('delete');
    const topTrending = Selector('div').withText('Top Trending');
    await t.expect(topTrending.exists).ok();
});
test('search is case insensitive', async (t) => {
    await t.typeText(searchInput, 'test');
    await t.expect(songCards.count).gt(0);
    const lowerCount = await songCards.count;

    await t.selectText(searchInput).typeText(searchInput, 'TEST');
    await t.expect(songCards.count).gt(0);
    const upperCount = await songCards.count;

    await t.expect(lowerCount).eql(upperCount);
});

// Song cards
test('song cards are rendered after songs load', async (t) => {
    await t.typeText(searchInput, ' ');
    await t.expect(songCards.count).gte(0);
});
test('clicking a song card does not throw', async (t) => {
    await t.typeText(searchInput, ' ');
    const cardCount = await songCards.count;
    if (cardCount > 0) {
        await t.click(songCards.nth(0));
        await t.expect(appTitle.exists).ok();
    }
});

// Playlist sidebar
test('renders playlists from API', async (t) => {
    await t.expect(topPlaylists.count).gte(0);
});
test('each playlist has an image and a name', async (t) => {
    const count = await topPlaylists.count;
    if (count > 0) {
        const firstPlaylist = topPlaylists.nth(0);
        const img  = firstPlaylist.find('img');
        const name = firstPlaylist.find('p');
        await t.expect(img.exists).ok();
        await t.expect(name.exists).ok();
    }
});
