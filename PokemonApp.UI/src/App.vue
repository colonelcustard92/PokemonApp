<template>
  <div class="app-container">
    <div class="content-wrapper">
      <h1 class="text-center mb-4">Pokédex</h1>

      <div v-if="!token" class="card mx-auto" style="max-width: 400px;">
        <div class="card-body">
          <h5 class="card-title">Login</h5>
          <div class="mb-3">
            <label class="form-label">Username</label>
            <input v-model="username" type="text" class="form-control" />
          </div>
          <div class="mb-3">
            <label class="form-label">Password</label>
            <input v-model="password" type="password" class="form-control" />
          </div>
          <button @click="login" class="btn btn-primary w-100">Login</button>
          <div v-if="error" class="alert alert-danger mt-3">{{ error }}</div>
        </div>
      </div>

      <div v-else>
        <div class="input-group mb-4">
          <input v-model="pokemonName"
                 @keyup.enter="fetchPokemon"
                 type="text"
                 class="form-control"
                 placeholder="Enter Pokémon name" />
          <button class="btn btn-outline-secondary" @click="fetchPokemon">
            Search
          </button>
        </div>

        <div v-if="pokemonData" class="card mx-auto" style="max-width: 400px;">
          <img :src="pokemonData.sprites.frontDefault"
               class="card-img-top"
               :alt="pokemonData.name" />
          <div class="card-body">
            <h5 class="card-title text-capitalize">{{ pokemonData.name }}</h5>
            <p class="card-text">
              <strong>Height:</strong> {{ pokemonData.height }}<br />
              <strong>Weight:</strong> {{ pokemonData.weight }}<br />
              <strong>Abilities:</strong>
              <ul>
                <li v-for="(ability, index) in pokemonData.abilities"
                    :key="index"
                    class="text-capitalize">
                  {{ ability.ability.name }} <em>({{ ability.isHidden ? "Hidden" : "Visible" }})</em>
                </li>
              </ul>
              <strong>Types:</strong>
              <ul>
                <li v-for="(type, index) in pokemonData.types"
                    :key="index"
                    class="text-capitalize">
                  {{ type.type.name }}
                </li>
              </ul>
            </p>
          </div>
        </div>

        <div v-else-if="pokemonNotFound" class="alert alert-warning text-center">
          Pokémon not found.
        </div>

        <div v-else-if="error" class="alert alert-danger text-center">
          {{ error }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref } from "vue";

  const username = ref("");
  const password = ref("");
  const token = ref("");
  const error = ref("");
  const pokemonName = ref("");
  const pokemonData = ref<any>(null);
  const pokemonNotFound = ref(false);

  async function login() {
    error.value = "";
    if (!username.value || !password.value) {
      error.value = "Please enter username and password";
      return;
    }

    try {
      const res = await fetch("https://localhost:7004/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          username: username.value,
          password: password.value,
        }),
      });

      if (!res.ok) throw new Error("Login failed");
      const data = await res.json();
      token.value = data.token;
    } catch (err: any) {
      error.value = err.message || "Login error";
    }
  }

  async function fetchPokemon() {
    error.value = "";
    pokemonData.value = null;
    pokemonNotFound.value = false;

    if (!pokemonName.value) {
      error.value = "Please enter a Pokémon name";
      return;
    }

    try {
      const res = await fetch(
        `https://localhost:7004/?name=${pokemonName.value}`,
        {
          headers: {
            Authorization: `Bearer ${token.value}`,
          },
        }
      );

      if (res.status === 404) {
        pokemonNotFound.value = true;
        return;
      }

      if (!res.ok) throw new Error("Failed to fetch Pokémon");

      pokemonData.value = await res.json();
    } catch (err: any) {
      error.value = err.message || "Error fetching Pokémon";
    }
  }
</script>

<style scoped>
  body {
    background-color: #f8f9fa;
    margin: 0;
    height: 100vh;
  }

  .app-container {
    height: 100vh;
    display: flex;
    justify-content: center; 
    align-items: center; 
    padding: 15px; 
    box-sizing: border-box;
  }

  .content-wrapper {
    max-width: 400px;
    width: 100%;
  }
</style>
