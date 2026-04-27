import { Selector, ClientFunction } from 'testcafe';

const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';

const appTitle       = Selector('span').withText('Music App');
const pageTitle      = Selector('h2').withText('Upload New Song');
const titleInput     = Selector('input[placeholder="Enter song title"]');
const artistInput    = Selector('input[placeholder="Enter artist name"]');
const songFileInput  = Selector('input[type="file"][accept="audio/mpeg"]');
const imageFileInput = Selector('input[type="file"][accept="image/jpeg,image/png,image/webp"]');
const isPublicToggle = Selector('input[type="checkbox"].toggle');
const uploadButton   = Selector('button').withText('Upload Song');
const imagePreview   = Selector('img[alt="Cover preview"]');
const imagePlaceholder = Selector('div').withText('🎵');
const addImageLabel  = Selector('span').withText('Add Image');
const changeImageLabel = Selector('span').withText('Change Image');
const imageFilename  = Selector('p').withAttribute('class', /text-center text-xs/);

fixture('Upload Song Page')
    .page(`${BASE_URL}/uploadSong`)
    .skipJsErrors(true)
    .beforeEach(async (t) => {
        await t.eval(() => {
            localStorage.setItem('token', 'test-token');
        });
        await t.navigateTo(`${BASE_URL}/uploadSong`);
    });

// Layout
test('renders app title', async (t) => {
    await t.expect(appTitle.exists).ok();
});

test('renders page title', async (t) => {
    await t.expect(pageTitle.exists).ok();
});

test('renders all form fields', async (t) => {
    await t.expect(titleInput.exists).ok();
    await t.expect(artistInput.exists).ok();
    await t.expect(songFileInput.exists).ok();
    await t.expect(imageFileInput.exists).ok();
    await t.expect(isPublicToggle.exists).ok();
    await t.expect(uploadButton.exists).ok();
});

test('upload button is enabled by default', async (t) => {
    await t.expect(uploadButton.hasAttribute('disabled')).notOk();
});

// Form inputs
test('can type into title input', async (t) => {
    await t.typeText(titleInput, 'My Test Song');
    await t.expect(titleInput.value).eql('My Test Song');
});

test('can type into artist input', async (t) => {
    await t.typeText(artistInput, 'Test Artist');
    await t.expect(artistInput.value).eql('Test Artist');
});

test('title and artist inputs start empty', async (t) => {
    await t.expect(titleInput.value).eql('');
    await t.expect(artistInput.value).eql('');
});

test('song file input only accepts audio/mpeg', async (t) => {
    await t.expect(songFileInput.getAttribute('accept')).eql('audio/mpeg');
});

test('image file input only accepts jpeg, png, webp', async (t) => {
    await t.expect(imageFileInput.getAttribute('accept')).eql('image/jpeg,image/png,image/webp');
});

// Toggle
test('visibility toggle is off by default', async (t) => {
    await t.expect(isPublicToggle.checked).notOk();
});

test('clicking visibility toggle turns it on', async (t) => {
    await t.click(isPublicToggle);
    await t.expect(isPublicToggle.checked).ok();
});

test('clicking visibility toggle twice returns it to off', async (t) => {
    await t.click(isPublicToggle);
    await t.click(isPublicToggle);
    await t.expect(isPublicToggle.checked).notOk();
});

// Image preview
test('shows music note placeholder before image is selected', async (t) => {
    await t.expect(imagePlaceholder.exists).ok();
});

test('shows Add Image label before image is selected', async (t) => {
    await t.expect(addImageLabel.exists).ok();
});

test('image preview is not shown before image is selected', async (t) => {
    await t.expect(imagePreview.exists).notOk();
});

test('filename is not shown before image is selected', async (t) => {
    await t.expect(imageFilename.exists).notOk();
});

// Upload error
test('shows alert when uploading without a file', async (t) => {
    await t.setNativeDialogHandler(() => true);
    await t.click(uploadButton);

    const dialogHistory = await t.getNativeDialogHistory();
    await t.expect(dialogHistory[0].type).eql('alert');
    await t.expect(dialogHistory[0].text).eql('Please select a song file.');
});
