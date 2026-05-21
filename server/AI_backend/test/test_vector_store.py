from unittest.mock import patch

import pytest

import tools.vector_store as vs


@pytest.fixture(autouse=True)
def clear_store():
    """Fixture to ensure the global in-memory store is reset before and after every test."""
    vs._store = []
    yield
    vs._store = []


class TestEmbedText:
    @patch("tools.vector_store.embed_text")
    def test_embed_text_calls_ollama(self, mock_embed_text):
        """Verify that vector_store routes queries out to the embed_text utility function."""
        mock_embed_text.return_value = [0.1, 0.2, 0.3]

        result = vs.embed_text("happy mood")

        assert result == [0.1, 0.2, 0.3]
        mock_embed_text.assert_called_once_with("happy mood")


class TestIndexSongs:
    @patch("tools.vector_store.embed_text")
    def test_index_songs_populates_store(self, mock_embed_text):
        """Verify that input songs are correctly transformed and saved to the global _store."""
        mock_embed_text.return_value = [1.0, 0.0]
        sample_songs = [
            {"id": "1", "title": "Song A", "mood": "energetic"},
            {"id": "2", "title": "Song B"}
        ]

        vs.index_songs(sample_songs)

        assert len(vs._store) == 2

        assert vs._store[0]["text"] == "energetic"
        assert vs._store[0]["embedding"] == [1.0, 0.0]
        assert vs._store[0]["song"] == sample_songs[0]

        assert vs._store[1]["text"] == "unknown"
        mock_embed_text.assert_any_call("unknown")


class TestCosineSimilarity:
    def test_exact_match(self):
        """Identical vectors should yield a cosine similarity of 1.0."""
        vec = [1.0, 2.0, 3.0]
        similarity = vs._cosine_similarity(vec, vec)
        assert similarity == pytest.approx(1.0)

    def test_orthogonal_vectors(self):
        """Perpendicular vectors should yield a cosine similarity of 0.0."""
        vec_a = [1.0, 0.0]
        vec_b = [0.0, 1.0]
        similarity = vs._cosine_similarity(vec_a, vec_b)
        assert similarity == pytest.approx(0.0)

    def test_opposite_vectors(self):
        """Directly opposing vectors should yield a cosine similarity of -1.0."""
        vec_a = [1.0, 0.0]
        vec_b = [-1.0, 0.0]
        similarity = vs._cosine_similarity(vec_a, vec_b)
        assert similarity == pytest.approx(-1.0)

    def test_zero_vector_edge_case(self):
        """If a vector has a magnitude of zero, it should return 0.0 instead of throwing a DivisionByZero error."""
        vec_a = [0.0, 0.0]
        vec_b = [1.0, 2.0]
        similarity = vs._cosine_similarity(vec_a, vec_b)
        assert similarity == pytest.approx(0.0)


class TestSimilaritySearch:
    def test_returns_top_k_sorted_results(self):
        """Verify search picks the highest scoring vectors and respects the top_k parameter constraint."""
        song_1 = {"title": "Perfect Match"}
        song_2 = {"title": "Close Match"}
        song_3 = {"title": "No Match"}

        vs._store = [
            {"song": song_1, "embedding": [1.0, 0.0], "text": "happy"},
            {"song": song_3, "embedding": [-1.0, 0.0], "text": "sad"},
            {"song": song_2, "embedding": [0.707, 0.707], "text": "neutral"},
        ]

        query = [1.0, 0.0]

        results = vs.similarity_search(query_embedding=query, top_k=2)

        assert len(results) == 2
        assert results[0] == song_1
        assert results[1] == song_2
        assert song_3 not in results